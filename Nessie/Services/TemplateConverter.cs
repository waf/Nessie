using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services
{
    /// <summary>
    /// Liquid template converter, using DotLiquid library
    /// </summary>
    class TemplateConverter
    {
        /// <summary>
        /// Run the liquid templates.
        /// An input file may export variables for other templates. These variables are returned as a Hash.
        /// </summary>
        /// <param name="file">the file to process</param>
        /// <param name="templateValues">the variables available to the input file</param>
        /// <returns>any variables this file makes available to other files</returns>
        internal string Convert(string html, Dictionary<string, Object> templateValues, out Hash templateOutput)
        //internal string Convert(string html, Dictionary<string, IBuffer<Hash>> templateValues, out Hash templateOutput)
        {
            var template = Template.Parse(html);
            string itemOutput = template.RenderFromStringDictionary(templateValues);
            templateOutput = template.InstanceAssigns;
            return itemOutput;
        }

        internal string Convert(string html, Hash templateValues, out Hash templateOutput)
        //internal string Convert(string html, Dictionary<string, IBuffer<Hash>> templateValues, out Hash templateOutput)
        {
            var template = Template.Parse(html);
            string itemOutput = template.Render(templateValues);
            templateOutput = template.InstanceAssigns;
            return itemOutput;
        }
    }
}
