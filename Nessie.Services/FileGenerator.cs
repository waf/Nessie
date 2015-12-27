using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services
{
    public class FileGenerator
    {
        private readonly TemplateConverter templater;
        private readonly MarkdownConverter markdown;

        public FileGenerator(TemplateConverter templater = null, MarkdownConverter markdown = null)
        {
            this.templater = templater ?? new TemplateConverter();
            this.markdown = markdown ?? new MarkdownConverter();
        }

        public FileOutput GenerateFile(FileLocation inputFileLocation, string inputContent, string[] templates, Dictionary<string, IBuffer<Hash>> projectVariables)
        {
            Hash fileVariables;
            
            // all files are transformed by the templater
            string fileOutput = templater.Convert(inputContent, projectVariables.AsTemplateValues(), out fileVariables);

            // markdown files get some additional processing
            if (inputFileLocation.Extension == ".md")
            {
                fileOutput = TransformMarkdownFile(fileOutput, projectVariables, fileVariables, templates);
            }

            var outputLocation = CreateOutputFileName(inputFileLocation, fileVariables);
            return new FileOutput(outputLocation, fileOutput, fileVariables);
        }

        private string TransformMarkdownFile(string fileContents, Dictionary<string, IBuffer<Hash>> projectVariables, Hash fileVariables, string[] templates)
        {
            if (!string.IsNullOrWhiteSpace(fileContents))
            {
                fileContents = markdown.Convert(fileContents);
            }
            else //empty output, so the file just exported variables. run the variables through the html templates until we get output
            {
                Hash exports = fileVariables
                    .ToDictionary(kvp => kvp.Key, kvp => markdown.Convert(kvp.Value.ToString()))
                    .AsTemplateValues();
                exports.Merge(projectVariables.AsTemplateValues());

                foreach (var template in templates)
                {
                    Hash iterationExports;
                    fileContents = templater.Convert(template, exports, out iterationExports);
                    exports.Merge(iterationExports);
                    if (!string.IsNullOrWhiteSpace(fileContents))
                    {
                        break;
                    }
                }
            }

            return fileContents;
        }

        private FileLocation CreateOutputFileName(FileLocation file, Hash variables)
        {
            object outputPattern;
            string prefix =
                variables.TryGetValue("nessie-url-prefix", out outputPattern) ?
                templater.Convert((string)outputPattern, variables) :
                file.Directory;

            string filename = file.Category == "" ?
                file.FileNameWithoutExtension :
                file.FileNameWithoutExtension.Replace($"_{file.Category}_", "");

            return new FileLocation(prefix, filename, file.Extension == ".md" ? ".html" : file.Extension);
        }
    }
}
