using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nessie.Services
{
    public class ProjectGenerator
    {
        readonly ReadFromFile ReadFile;
        readonly WriteToFile WriteFile;
        readonly FileGenerator fileGenerator;
        static readonly string[] FileTypesToTransform = {
            ".md", ".json", ".xml", ".txt", ".html"
        };

        public ProjectGenerator(ReadFromFile readFile, WriteToFile writeFile, FileGenerator fileGenerator)
        {
            this.ReadFile = readFile;
            this.WriteFile = writeFile;
            this.fileGenerator = fileGenerator;
        }

        public void Generate(string inputRoot, IList<string> inputFiles, string outputRoot)
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
            TransformFiles(inputRoot, outputRoot, transformableFiles[true].ToArray(), templates);

            // just copy the files we can't handle to the output root
            CopyFiles(inputRoot, outputRoot, transformableFiles[false].ToArray());
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


        private static void CopyFiles(string inputRoot, string outputRoot, FileLocation[] files)
        {
            foreach (var file in files)
            {
                string outputFileRelativeToInputRoot = MakeFileRelativeToPath(file, inputRoot);
                string outputLocation = Path.Combine(outputRoot, outputFileRelativeToInputRoot);
                Console.WriteLine($"Copying {outputFileRelativeToInputRoot}");
                Directory.CreateDirectory(Path.GetDirectoryName(outputLocation));
                File.Copy(file.FullyQualifiedName, outputLocation, true);
            }
        }

        private void TransformFiles(string inputRoot, string outputRoot, FileLocation[] files, FileLocation[] templates)
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
                                        inputRoot,
                                        file,
                                        ReadFile(file.FullyQualifiedName),
                                        GetApplicableTemplates(templates, file).Select(template => ReadFile(template.FullyQualifiedName)).ToArray(),
                                        allFileVariables)
                                     )
                                    .Do(outputFile => WriteFileAndDirectories(outputRoot, inputRoot, outputFile.Name, outputFile.Output))
                                    .Select(context => context.Variables)
                                    .Memoize());

            // evaluate the lazy values in the dictionary, to generate all the files.
            foreach (var item in allFileVariables)
            {
                item.Value.ToList();
            }
        }

        private void WriteFileAndDirectories(string outputRoot, string inputRoot, FileLocation file, string output)
        {
            if(string.IsNullOrWhiteSpace(output))
            {
                return;
            }
            string outputFileRelativeToInputRoot = MakeFileRelativeToPath(file, inputRoot);
            string outputLocation = Path.Combine(outputRoot, outputFileRelativeToInputRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(outputLocation));
            Console.WriteLine("Generating " + outputFileRelativeToInputRoot);
            WriteFile(outputLocation, output);
        }

        private static string MakeFileRelativeToPath(FileLocation file, string path)
        {
            return file.FullyQualifiedName.StartsWith(path)
                ? file.FullyQualifiedName.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar)
                : file.FullyQualifiedName;
        }
    }
}