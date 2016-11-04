using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nessie.Services
{
    using FileReaderFunction = Func<string, string>;
    using FileWriterFunction = Action<string, string>;
    public class ProjectGenerator
    {
        private readonly FileReaderFunction ReadFile;
        private readonly FileWriterFunction WriteFile;
        private readonly FileGenerator fileGenerator;
        private static readonly string[] FileTypesToTransform = {
            ".md", ".json", ".xml", ".txt", ".html"
        };
        public ProjectGenerator(
            string inputRoot,
            // poor-man's dependency injection for mocking purposes
            FileReaderFunction readFile = null, FileWriterFunction writeFile = null, FileGenerator fileGenerator = null)
        {
            this.ReadFile = readFile ?? File.ReadAllText;
            this.WriteFile = writeFile ?? File.WriteAllText;
            this.fileGenerator = fileGenerator ?? new FileGenerator(inputRoot);
        }

        private IList<FileLocation> GetApplicableTemplates(IList<FileLocation> allTemplates, FileLocation file)
        {
            // only markdown files have templates applied to them
            if(file.Extension != ".md")
            {
                return new List<FileLocation>();
            }

            var templatesWithCategory = (from template in allTemplates
                                         select new
                                         {
                                             template,
                                             category = template.FileNameWithoutExtension.Replace("_template_", "")
                                         }).ToArray();
            // if this file has a category, but there's no associated template, don't return any templates.
            // this prevents a item/_item_foo.md being rendered with a top-level template in a parent directory.
            if(!templatesWithCategory.Any() || (file.Category != string.Empty && templatesWithCategory.Last().category != file.Category))
            {
                return new List<FileLocation>();
            }

            var directories = file.Directory
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(new List<string> { "" },
                           (list, dir) =>
                           {
                               list.Add(Path.Combine(list.Last(), dir));
                               return list;
                           })
                .ToArray();

            var applicableTemplates = (from template in templatesWithCategory
                                       where directories.Contains(template.template.Directory) &&
                                         (template.category == "" || file.Category == template.category)
                                       orderby template.template.FullyQualifiedName.Length descending
                                       select template.template).ToList();

            return applicableTemplates.ToList();
        }

        public void Generate(string root, IList<string> inputFiles, string outputRoot)
        {
            // split file list into templates vs non-templates   
            var isTemplateFile = inputFiles
                .Select(file => new FileLocation(file))
                .ToLookup(file => file.Category == "template");
            var templates = isTemplateFile[true].ToArray();
            var files = isTemplateFile[false].ToArray();

            // split non-templates into "files to transform" vs "files to copy"
            var transformableFiles = files
                .ToLookup(file => FileTypesToTransform.Contains(file.Extension));

            // transform the "files to transform", passing in the templates
            TransformFiles(outputRoot, transformableFiles[true].ToArray(), templates);

            // just copy the files we can't handle to the output root
            CopyFiles(outputRoot, transformableFiles[false].ToArray());
        }

        private static void CopyFiles(string outputRoot, FileLocation[] files)
        {
            foreach (var file in files)
            {
                string targetLocation = CombineRelativePaths(outputRoot, file.FullyQualifiedName);
                Console.WriteLine($"Copying {targetLocation}");
                Directory.CreateDirectory(Path.GetDirectoryName(targetLocation));
                File.Copy(file.FullyQualifiedName, targetLocation, true);
            }
        }

        private void TransformFiles(string outputRoot, FileLocation[] files, FileLocation[] templates)
        {
            /* this next statement is a bit tricky, it takes advantage of laziness and a recursive variable definition.
               we want all file variables to be available to all other files when they're rendering.

               given files _post_a.md, _post_b.md, _product_x.md, it will generate a dictionary:
                    post => _post_a.md's variables, _post_b.md's variables
                    product => _product_x's variables
            */
            Dictionary<string, IBuffer<Hash>> allFileVariables = null;
            allFileVariables = files
                .Where(file => FileTypesToTransform.Contains(file.Extension))
                .GroupBy(file => file.Category) // this gets the template variable keys available for the templates
                .ToDictionary(
                    fileGroup => fileGroup.Key,
                    fileGroup => fileGroup
                                    .Select(file => fileGenerator.GenerateFile(
                                        file,
                                        ReadFile(file.FullyQualifiedName),
                                        GetApplicableTemplates(templates, file).Select(template => ReadFile(template.FullyQualifiedName)).ToArray(),
                                        allFileVariables)
                                     )
                                    .Do(outputFile => WriteFileAndDirectories(outputRoot, outputFile.Name, outputFile.Output))
                                    .Select(context => context.Variables)
                                    .Memoize());

            // evaluate the lazy values in the dictionary, to generate all the files.
            foreach (var item in allFileVariables)
            {
                item.Value.ToList();
            }
        }

        private void WriteFileAndDirectories(string outputRoot, FileLocation file, string output)
        {
            if(string.IsNullOrWhiteSpace(output))
            {
                return;
            }
            string directory = CombineRelativePaths(outputRoot, file.Directory);
            Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, file.FileNameWithoutExtension + file.Extension);
            Console.WriteLine($"Generating {filePath}");
            WriteFile(filePath, output);
        }

        private static string CombineRelativePaths(string path1, string path2)
        {
            // is there a better way to do this? feels like a kludge
            return Path
                .GetFullPath(Path.Combine(path1, path2))
                .Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, "");
        }
    }
}