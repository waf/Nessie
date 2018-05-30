
using McMaster.Extensions.CommandLineUtils;
using System;

namespace Nessie
{
    public static class CommandLineExtensions
    {
        public static int GetValueOrDefault(this CommandOption option, int defaultValue) =>
            ConditionallyParse(option, int.Parse, defaultValue);

        public static bool GetValueOrDefault(this CommandOption option, bool defaultValue) =>
            option.OptionType == CommandOptionType.NoValue
                ? option.HasValue()
                : ConditionallyParse(option, bool.Parse, defaultValue);

        public static string GetValueOrDefault(this CommandOption option, string defaultValue) =>
            ConditionallyParse(option, x => x, defaultValue);

        private static T ConditionallyParse<T>(CommandOption option, Func<string, T> parse, T defaultValue) =>
            option.HasValue()
                ? parse(option.Value())
                : defaultValue;
    }
}
