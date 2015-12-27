﻿using DotLiquid;
using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nessie.Services
{
    using FileReader = Func<string, string>;
    using FileWriter = Action<string, string>;
    public class ProjectGenerator
    {
        private readonly FileReader ReadFile;
        private readonly FileWriter WriteFile;
        private readonly FileGenerator fileGenerator;

        public ProjectGenerator(
            // poor-man's dependency injection for mocking purposes
            FileReader readFile = null, FileWriter writeFile = null, FileGenerator fileGenerator = null)
        {
            this.ReadFile = readFile ?? File.ReadAllText;
            this.WriteFile = writeFile ?? File.WriteAllText;
            this.fileGenerator = fileGenerator ?? new FileGenerator();
        }

        private IList<FileLocation> GetApplicableTemplates(IList<FileLocation> allTemplates, FileLocation file)
        {

            // foo/bar/baz/bang
            // foo/bar/baz/bang
            // foo/bar/baz
            // foo/bar
            // foo/
            var directories = file.Directory
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(new List<string> { "" },
                           (list, dir) =>
                           {
                               list.Add(Path.Combine(list.Last(), dir));
                               return list;
                           })
                .ToArray();

            var applicableTemplates = from template in allTemplates
                                      let templateCategory = template.FileNameWithoutExtension.Replace("_template_", "")
                                      where directories.Contains(template.Directory) &&
                                        (templateCategory == "" || file.Category == templateCategory)
                                      orderby template.FullyQualifiedName.Length descending
                                      select template;

            return applicableTemplates.ToList();
        }

        public void Generate(string root, IList<string> inputFiles, string outputRoot)
        {
            var files = inputFiles.Select(file => new FileLocation(file)).ToArray();

            var templates = files
                .Where(file => file.Category == "template")
                .ToArray();

            /* this next statement is a bit tricky, it takes advantage of laziness and a recursive variable definition.
               we want all template variables to be available to all templates when they're rendering.

               given files _post_a.md, _post_b.md, _product_x.md, it will generate a dictionary:
                    post => _post_a.md's variables, _post_b.md's variables
                    product => _product_x's variables
            */
            Dictionary<string, IBuffer<Hash>> allTemplateVariables = null;
            allTemplateVariables = files
                .Where(file => file.Extension == ".md")
                .GroupBy(file => file.Category) // this gets the template variable keys available for the templates
                .ToDictionary(
                    fileGroup => fileGroup.Key,
                    fileGroup => fileGroup
                                    .Select(file => fileGenerator.GenerateFile(
                                        file, 
                                        ReadFile(file.FullyQualifiedName), 
                                        GetApplicableTemplates(templates, file).Select(template => ReadFile(template.FullyQualifiedName)).ToArray(), 
                                        allTemplateVariables)
                                     )
                                    .Do(outputFile => WriteFileAndDirectories(outputRoot, outputFile.Name, outputFile.Output))
                                    .Select(context => context.Variables)
                                    .Memoize());

            // evaluate the lazy values in the dictionary, to generate all the files.
            foreach (var item in allTemplateVariables)
            {
                item.Value.ToList();
            }
        }

        private void WriteFileAndDirectories(string outputRoot, FileLocation file, string output)
        {
            string fullDirectory = Path.GetFullPath(Path.Combine(outputRoot, file.Directory));
            string fullPath = Path.GetFullPath(Path.Combine(outputRoot, file.FullyQualifiedName));
            Directory.CreateDirectory(fullDirectory);
            string displayPath = fullPath.Replace(Directory.GetCurrentDirectory(), "");
            Console.WriteLine($"Generating {displayPath}");
            WriteFile(fullPath, output);
        }
    }
}