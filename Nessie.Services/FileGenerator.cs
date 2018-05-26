using DotLiquid;
using Nessie.Services.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public FileGenerator(TemplateProcessor templater, MarkdownProcessor markdown)
        {
            this.templater = templater;
            this.markdown = markdown;
        }

        public FileOutput GenerateFile(
            string inputRoot,
            in FileLocation inputFileLocation,
            string inputContent,
            string[] templates,
            Dictionary<string, IBuffer<Hash>> projectVariables)
        {
            // all files are transformed by the templater
            string fileOutput = templater.Convert(inputRoot, inputContent, projectVariables.AsTemplateValues(), out Hash fileVariables);

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
            Dictionary<string, IBuffer<Hash>> projectVariables,
            Hash fileVariables)
        {
            if (!string.IsNullOrWhiteSpace(inputContent))
            {
                return markdown.Convert(inputContent);
            }

            //empty output, so the file just exported variables. run the variables through the html templates until we get output
            Hash exports = fileVariables
                .ToDictionary(kvp => kvp.Key, kvp =>
                {
                    string markdownText = kvp.Value.ToString();
                    string html = markdown.Convert(markdownText).Trim(NewLines);
                    return markdownText.Trim(NewLines).Length == markdownText.Length ? // if the input text has no wrapping whitespace
                            html.Substring(3, html.Length - 7) : // then trim wrapping paragraph tags
                            html;
                })
                .AsTemplateValues();
            exports.Merge(projectVariables.AsTemplateValues());

            foreach (var template in templates)
            {
                inputContent = templater.Convert(inputRoot, template, exports, out Hash iterationExports);
                exports.Merge(iterationExports);
                if (!string.IsNullOrWhiteSpace(inputContent))
                {
                    break;
                }
            }

            return inputContent;
        }

        private FileLocation CreateOutputFileName(string inputRoot, in FileLocation file, Hash variables)
        {
            string prefix = variables.TryGetVariable("nessie-url-prefix", out string outputPattern)
                ? templater.Convert(inputRoot, outputPattern, variables)
                : file.Directory;

            string filename = file.Category == ""
                ? file.FileNameWithoutExtension
                : file.FileNameWithoutExtension.Replace($"_{file.Category}_", "");

            return new FileLocation(prefix, filename, file.Extension == ".md" ? ".html" : file.Extension);
        }
    }
}
