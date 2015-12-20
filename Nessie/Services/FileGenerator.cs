using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services
{
    class FileGenerator
    {
        private readonly TemplateConverter templater;
        private readonly MarkdownConverter markdown;

        public FileGenerator(TemplateConverter templater = null, MarkdownConverter markdown = null)
        {
            this.templater = templater ?? new TemplateConverter();
            this.markdown = markdown ?? new MarkdownConverter();
        }

        public TemplateOutput GenerateFile(FileLocation inputFileLocation, string inputContent, string autoContent, Dictionary<string, IBuffer<Hash>> allTemplateVariables)
        {
            Hash templateVariableExports;
            string templateResult = templater.Convert(inputContent, allTemplateVariables, out templateVariableExports);
            string fileOutput;
            if (!string.IsNullOrWhiteSpace(templateResult))
            {
                fileOutput = markdown.Convert(templateResult);
            }
            else
            {
                var markdownedHashes = templateVariableExports.ToDictionary(kvp => kvp.Key, kvp => markdown.Convert(kvp.Value.ToString()));
                fileOutput = Template.Parse(autoContent).RenderFromStringDictionary(markdownedHashes);
            }
            var outputLocation = CreateOutputFileName(inputFileLocation);
            return new TemplateOutput(outputLocation, fileOutput, templateVariableExports);
        }

        private FileLocation CreateOutputFileName(FileLocation file)
        {
            return new FileLocation(file.Directory, file.FileNameWithoutExtension, "html");
        }
    }
}
