using Nessie.Services.Models;
using Nessie.Services.Processors;
using System.Collections.Generic;
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
            string fileOutput = templater.Convert(inputRoot, inputContent, projectVariables.AsTemplateValues(), out var fileVariables);

            var environment = Settings.Default;
            environment = environment.SetItems(projectVariables.AsTemplateValues());

            // markdown files get some additional processing
            if (inputFileLocation.Extension == ".md")
            {
                environment = environment.SetItems(TransformMarkdownVariables(fileVariables, environment));
                var processOutput = TransformMarkdownFile(inputRoot, fileOutput, templates, environment);
                fileOutput = processOutput.Output;
                environment = processOutput.Variables;
            }
            else
            {
                environment = environment.SetItems(fileVariables);
            }

            var outputLocation = CreateOutputFileName(inputRoot, inputFileLocation, environment);
            environment = environment.TrySetItem(Settings.ItemUrl, outputLocation.FullyQualifiedName);

            return new FileOutput(outputLocation, fileOutput, environment);
        }

        private ProcessOutput TransformMarkdownFile(
            string inputRoot,
            string inputContent,
            IReadOnlyList<string> templates,
            ImmutableDictionary<string, object> environment
        )
        {
            if (!string.IsNullOrWhiteSpace(inputContent) || templates.Count == 0)
            {
                var markdownContent = markdown.Convert(inputContent, environment);
                return new ProcessOutput(markdownContent, environment);
            }

            inputContent = templater.Convert(inputRoot, templates.First(), environment, out var iterationExports);
            environment = environment.SetItems(iterationExports);
            return TransformMarkdownFile(inputRoot, inputContent, templates.Skip(1).ToArray(), environment);
        }

        private ImmutableDictionary<string, object> TransformMarkdownVariables(ImmutableDictionary<string, object> fileVariables, ImmutableDictionary<string, object> environment)
        {
            var exports = fileVariables
                .ToDictionary(kvp => kvp.Key, kvp =>
                {
                    string markdownText = kvp.Value.ToString();
                    string html = markdown.Convert(markdownText, environment).Trim(NewLines);
                    return markdownText.Trim(NewLines).Length == markdownText.Length ? // if the input text has no wrapping whitespace
                            html.Substring(3, html.Length - 7) : // then trim wrapping paragraph tags
                            html as object;
                })
                .ToImmutableDictionary();
            return exports;
        }

        private FileLocation CreateOutputFileName(string inputRoot, FileLocation file, ImmutableDictionary<string, object> environment)
        {
            string prefix = environment.TryGetValue(Settings.UrlPrefix, out object outputPattern)
                ? templater.Convert(inputRoot, outputPattern.ToString(), environment)
                : file.Directory;

            string filename = file.Category == ""
                ? file.FileNameWithoutExtension
                : file.FileNameWithoutExtension.Replace($"_{file.Category}_", "");

            return new FileLocation(prefix, filename, file.Extension == ".md" ? ".html" : file.Extension);
        }
    }
}
