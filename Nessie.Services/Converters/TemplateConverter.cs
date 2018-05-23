using DotLiquid;
using DotLiquid.FileSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services.Converters
{
    /// <summary>
    /// Liquid template converter, using DotLiquid library
    /// </summary>
    public class TemplateConverter
    {
        /// <summary>
        /// Processes the Liquid template tags in a string.
        /// </summary>
        /// <param name="input">The input string, with Liquid template tags</param>
        /// <param name="inputVariables">the variables available to the input file</param>
        /// <returns>The output string with all template tags processed</returns>
        internal string Convert(string inputRoot, string input, Hash inputVariables)
        {
            Hash _;
            return Convert(inputRoot, input, inputVariables, out _);
        }

        /// <summary>
        /// Processes the Liquid template tags in an HTML file.
        /// </summary>
        /// <param name="html">The input HTML, with Liquid template tags</param>
        /// <param name="inputVariables">the variables available to the input file</param>
        /// <param name="outputVariables">any variables that this file sets</param>
        /// <returns>The HTML, with all template tags processed</returns>
        internal string Convert(string inputRoot, string html, Hash inputVariables, out Hash outputVariables)
        {
            if(Template.FileSystem == null)
            {
                string absoluteRoot = Path.GetFullPath(inputRoot);
                Template.FileSystem = new NessieLiquidFileSystem(absoluteRoot);
            }
            var template = Template.Parse(html);
            string itemOutput = template.Render(inputVariables);
            outputVariables = template.InstanceAssigns;
            return itemOutput;
        }
    }
}
