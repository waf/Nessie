using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nessie.Services
{
    public static class Settings
    {
        public static readonly ImmutableDictionary<string, object> Default =
            new Dictionary<string, object>
            {
                { PandocSettings, "-f markdown -t html" }
            }
            .ToImmutableDictionary();

        /// <summary>
        /// Pandoc command line arguments
        /// </summary>
        public const string PandocSettings = "nessie-pandoc-settings";

        /// <summary>
        /// The URL of the generated output for this item
        /// </summary>
        public const string ItemUrl = "nessie-item-url";

        /// <summary>
        /// A prefix, e.g. '/posts/' for the generated output file.
        /// Used if the output file structure does not match the input file structure.
        /// </summary>
        public const string UrlPrefix = "nessie-url-prefix";
    }
}
