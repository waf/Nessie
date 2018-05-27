using System.CommandLine;

namespace Nessie
{
    static class CommandLineExtensions
    {
        public static Argument<int> DefineOption(this ArgumentSyntax syntax, string name, int defaultValue, string help)
        {
            var arg = syntax.DefineOption(name, defaultValue);
            arg.Help = help;
            return arg;
        }

        public static Argument<bool> DefineOption(this ArgumentSyntax syntax, string name, bool defaultValue, string help)
        {
            var arg = syntax.DefineOption(name, defaultValue);
            arg.Help = help;
            return arg;
        }

        public static Argument<string> DefineOption(this ArgumentSyntax syntax, string name, string defaultValue, string help)
        {
            var arg = syntax.DefineOption(name, defaultValue);
            arg.Help = help;
            return arg;
        }
    }
}
