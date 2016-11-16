using Nessie.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.CommandLine;

namespace Nessie.Commands
{
    class BuildCommand : ICommand
    {
        // TODO: these should be configurable via command line flags
        public string Name => "build";
        public const string DefaultOutputDirectory = ".\\_output";
        static string source = ".";
        static string destination = DefaultOutputDirectory;

        public int Run()
        {
            var timer = Stopwatch.StartNew();
            var files = Directory
                .GetFiles(source, "*", SearchOption.AllDirectories)
                .Where(file => {
                    bool isHiddenFile = new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden);
                    bool isOutputFile = file.StartsWith(DefaultOutputDirectory, StringComparison.CurrentCulture);
                    return !(isHiddenFile || isOutputFile);
                 })
                .ToList();

            var generator = new ProjectGenerator(source);
            generator.Generate(source, files, destination);

            Console.WriteLine($"Built site in {timer.ElapsedMilliseconds} milliseconds");
            return 0;
        }

        public void DefineArguments(ArgumentSyntax syntax, ref string command)
        {
            syntax.DefineCommand(Name, ref command, "generates the static site");
        }
    }
}