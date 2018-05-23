using DotLiquid;
using Nessie.Services.Converters;
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
        private readonly TemplateConverter templater;
        private readonly MarkdownConverter markdown;
        private static readonly char[] NewLines =  { '\r', '\n' };

        public FileGenerator(TemplateConverter templater, MarkdownConverter markdown)
        {
            this.templater = templater;
            this.markdown = markdown;
        }

        public FileOutput GenerateFile(string inputRoot, FileLocation inputFileLocation, string inputContent, string[] templates, Dictionary<string, IBuffer<Hash>> projectVariables)
        {
            // all files are transformed by the templater
            string fileOutput = templater.Convert(inputRoot, inputContent, projectVariables.AsTemplateValues(), out Hash fileVariables);

            // markdown files get some additional processing
            if (inputFileLocation.Extension == ".md")
            {
                fileOutput = TransformMarkdownFile(inputRoot, fileOutput, projectVariables, fileVariables, templates);
            }

            var outputLocation = CreateOutputFileName(inputRoot, inputFileLocation, fileVariables);

            return new FileOutput(outputLocation, fileOutput, fileVariables);
        }

        private string TransformMarkdownFile(string inputRoot, string fileContents, Dictionary<string, IBuffer<Hash>> projectVariables, Hash fileVariables, string[] templates)
        {
            if (!string.IsNullOrWhiteSpace(fileContents))
            {
                return markdown.Convert(fileContents);
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
                Hash iterationExports;
                fileContents = templater.Convert(inputRoot, template, exports, out iterationExports);
                exports.Merge(iterationExports);
                if (!string.IsNullOrWhiteSpace(fileContents))
                {
                    break;
                }
            }

            return fileContents;
        }

        private FileLocation CreateOutputFileName(string inputRoot, FileLocation file, Hash variables)
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
