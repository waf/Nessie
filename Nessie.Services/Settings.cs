using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nessie.Services
{
    public static class Settings
    {
        public static readonly ImmutableDictionary<string, object> Default =
            new Dictionary<string, object>
            {
                { MarkdownSettings, "-f markdown -t html" }
            }
            .ToImmutableDictionary();

        public const string MarkdownSettings = "nessie-markdown-settings";
        public const string ItemUrl = "nessie-item-url";
    }
}
