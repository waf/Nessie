using DotLiquid;
using DotLiquid.FileSystems;
using Nessie.Services.Utils;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Nessie.Services.Processors
{
    /// <summary>
    /// Liquid template converter, using DotLiquid library
    /// </summary>
    public class TemplateProcessor
    {
        private readonly FileOperation fileio;

        public TemplateProcessor() : this(new FileOperation()) { }

        public TemplateProcessor(FileOperation fileio)
        {
            this.fileio = fileio;
        }

        /// <summary>
        /// Processes the Liquid template tags in a string.
        /// </summary>
        /// <param name="inputRoot">The directory root to which includes are relative</param>
        /// <param name="input">The input string, with Liquid template tags</param>
        /// <param name="inputVariables">the variables available to the input file</param>
        /// <returns>The output string with all template tags processed</returns>
        public string Convert(string inputRoot, string input, ImmutableDictionary<string, object> inputVariables)
        {
            return Convert(inputRoot, input, inputVariables, out var _);
        }

        /// <summary>
        /// Processes the Liquid template tags in a string
        /// </summary>
        /// <param name="inputRoot">The directory root to which includes are relative</param>
        /// <param name="input">The input HTML, with Liquid template tags</param>
        /// <param name="inputVariables">the variables available to the input file</param>
        /// <param name="outputVariables">any variables that this file sets</param>
        /// <returns>The HTML, with all template tags processed</returns>
        public string Convert(string inputRoot, string input, ImmutableDictionary<string, object> inputVariables, out ImmutableDictionary<string, object> outputVariables)
        {
            if (Template.FileSystem is BlankFileSystem)
            {
                string absoluteRoot = Path.GetFullPath(inputRoot);
                Template.FileSystem = new NessieLiquidFileSystem(fileio, absoluteRoot);
            }
            var template = Template.Parse(input);
            string itemOutput = template.Render(ConvertToHash(inputVariables));
            outputVariables = template.InstanceAssigns.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return itemOutput;
        }

        private static Hash ConvertToHash(IImmutableDictionary<string, object> inputVariables)
        {
            return Hash.FromDictionary(inputVariables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}
