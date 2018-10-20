using Nessie.Services.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public ProjectGenerator() : this(new FileOperation(), new FileGenerator(), new TemplateService()) { }

        public ProjectGenerator(FileOperation fileio, FileGenerator fileGenerator, TemplateService templateService)
        {
            this.fileio = fileio;
            this.fileGenerator = fileGenerator;
            this.templateService = templateService;
        }

        public void Generate(string inputRoot, string outputRoot, IList<string> files)
        {
            var filesByTransformType = files
                .Select(file => new FileLocation(file))
                .ToLookup(file => GetTransformType(file));

            var inputFiles = filesByTransformType[TransformType.Input].ToArray();
            var templates = filesByTransformType[TransformType.Template].ToArray();
            GenerateFiles(inputRoot, outputRoot, inputFiles, templates);

            var filesToCopy = filesByTransformType[TransformType.Copy].ToArray();
            fileio.CopyFiles(inputRoot, outputRoot, filesToCopy);
        }

        private static TransformType GetTransformType(FileLocation file)
        {
            return file.Category == "template" ? TransformType.Template :
                   FileTypesToTransform.Contains(file.Extension) ? TransformType.Input :
                   TransformType.Copy;
        }

        private void GenerateFiles(string inputRoot, string outputRoot,
            IReadOnlyCollection<FileLocation> files, IReadOnlyCollection<FileLocation> allTemplates)
        {
            /* this next statement is a bit tricky, it takes advantage of laziness and a recursive variable definition.
               we want all file variables to be available to all other files when they're rendering.

               given files _post_a.md, _post_b.md, _product_x.md, it will generate a dictionary:
                    post => _post_a.md's variables, _post_b.md's variables
                    product => _product_x's variables
            */
            ImmutableDictionary<string, IBuffer<ImmutableDictionary<string, object>>> allFileVariables = null;
            allFileVariables = files
                .GroupBy(file => file.Category) // this gets the template variable keys available for the templates
                .ToImmutableDictionary(
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

            void ForceEvaluation(IBuffer<ImmutableDictionary<string, object>> buffer) => buffer.ToList();
        }

        private enum TransformType
        {
            /// <summary>
            /// File will be an input to the generation process
            /// </summary>
            Input,
            /// <summary>
            /// File will act as a template for other files during the generation process
            /// </summary>
            Template,
            /// <summary>
            /// File will be copied, without modification, to the output directory
            /// </summary>
            Copy
        }
    }
}