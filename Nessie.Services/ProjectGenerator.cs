using DotLiquid;
using Nessie.Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nessie.Services
{
    public class ProjectGenerator
    {
        private readonly FileOperation fileio;
        private readonly FileGenerator fileGenerator;
        private readonly TemplateService templateService;

        private static readonly ISet<string> FileTypesToTransform = new HashSet<string> {
            ".md", ".json", ".xml", ".txt", ".html"
        };

        public ProjectGenerator(FileOperation fileio, FileGenerator fileGenerator, TemplateService templateService)
        {
            this.fileio = fileio;
            this.fileGenerator = fileGenerator;
            this.templateService = templateService;
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
            fileio.CopyFiles(inputRoot, outputRoot, transformableFiles[false].ToArray());
        }

        private void TransformFiles(string inputRoot, string outputRoot, FileLocation[] files, FileLocation[] allTemplates)
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
                        .Select(file =>
                        {
                            string content = fileio.ReadFile(file.FullyQualifiedName);
                            string[] templates = templateService.GetApplicableTemplates(allTemplates, file)
                                .Select(template => fileio.ReadFile(template.FullyQualifiedName))
                                .ToArray();
                            var output = fileGenerator.GenerateFile(inputRoot, file, content, templates, allFileVariables);
                            return output;
                        })
                        .Do(outputFile => fileio.WriteFileAndDirectories(outputRoot, inputRoot, outputFile.Name, outputFile.Output))
                        .Select(context => context.Variables)
                        .Memoize()
                    );

            // evaluate the lazy values in the dictionary, to generate all the files.
            foreach (var item in allFileVariables)
            {
                ForceEvaluation(item.Value);
            }

            void ForceEvaluation(IBuffer<Hash> buffer) => buffer.ToList();
        }
    }
}