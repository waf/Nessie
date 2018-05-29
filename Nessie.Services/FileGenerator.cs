using Nessie.Services.Processors;
using System.Collections.Immutable;
using System.Linq;

namespace Nessie.Services
{
    /// <summary>
    /// Processes a single file.
    /// </summary>
    public class FileGenerator
    {
        private readonly TemplateProcessor templater;
        private readonly MarkdownProcessor markdown;
        private static readonly char[] NewLines =  { '\r', '\n' };

        public FileGenerator() : this(new TemplateProcessor(), new MarkdownProcessor()) { }

        public FileGenerator(TemplateProcessor templater, MarkdownProcessor markdown)
        {
            this.templater = templater;
            this.markdown = markdown;
        }

        public FileOutput GenerateFile(
            string inputRoot,
            FileLocation inputFileLocation,
            string inputContent,
            string[] templates,
            ImmutableDictionary<string, IBuffer<ImmutableDictionary<string, object>>> projectVariables)
        {
            // all files are transformed by the templater
            string fileOutput = templater.Convert(inputRoot, inputContent, projectVariables.AsTemplateValues(), out ImmutableDictionary<string, object> fileVariables);

            // markdown files get some additional processing
            if (inputFileLocation.Extension == ".md")
            {
                fileOutput = TransformMarkdownFile(inputRoot, fileOutput, templates, projectVariables, fileVariables);
            }

            var outputLocation = CreateOutputFileName(inputRoot, inputFileLocation, fileVariables);

            return new FileOutput(outputLocation, fileOutput, fileVariables);
        }

        private string TransformMarkdownFile(
            string inputRoot,
            string inputContent,
            string[] templates,
            ImmutableDictionary<string, IBuffer<ImmutableDictionary<string, object>>> projectVariables,
            ImmutableDictionary<string, object> fileVariables)
        {
            if (!string.IsNullOrWhiteSpace(inputContent))
            {
                return markdown.Convert(inputContent);
            }

            var environment = ImmutableDictionary.Create<string, object>();
            environment = environment.SetItems(projectVariables.AsTemplateValues());
            environment = environment.SetItems(ProcessMarkdown(fileVariables));

            foreach (var template in templates)
            {
                inputContent = templater.Convert(inputRoot, template, environment, out ImmutableDictionary<string, object> iterationExports);
                environment = environment.SetItems(iterationExports);
                if (!string.IsNullOrWhiteSpace(inputContent))
                {
                    break;
                }
            }

            return inputContent;
        }

        private ImmutableDictionary<string, object> ProcessMarkdown(ImmutableDictionary<string, object> fileVariables)
        {
            var exports = fileVariables
                .ToDictionary(kvp => kvp.Key, kvp =>
                {
                    string markdownText = kvp.Value.ToString();
                    string html = markdown.Convert(markdownText).Trim(NewLines);
                    return markdownText.Trim(NewLines).Length == markdownText.Length ? // if the input text has no wrapping whitespace
                            html.Substring(3, html.Length - 7) : // then trim wrapping paragraph tags
                            html as object;
                })
                .ToImmutableDictionary();
            return exports;
        }

        private FileLocation CreateOutputFileName(string inputRoot, FileLocation file, ImmutableDictionary<string, object> variables)
        {
            string prefix = variables.TryGetValue("nessie-url-prefix", out object outputPattern)
                ? templater.Convert(inputRoot, outputPattern.ToString(), variables)
                : file.Directory;

            string filename = file.Category == ""
                ? file.FileNameWithoutExtension
                : file.FileNameWithoutExtension.Replace($"_{file.Category}_", "");

            return new FileLocation(prefix, filename, file.Extension == ".md" ? ".html" : file.Extension);
        }
    }
}
