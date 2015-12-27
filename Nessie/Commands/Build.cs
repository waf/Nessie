using ManyConsole;
using Nessie.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Commands
{
    class Build : ConsoleCommand
    {
        public const string DefaultOutputDirectory = ".\\_output";
        private string source;
        private string destination;

        public Build()
        {
            this.source = ".";
            this.destination = DefaultOutputDirectory;

            IsCommand("build", "generates the static site");
        }

        public override int Run(string[] arguments)
        {
            var files = Directory
                .GetFiles(source, "*", SearchOption.AllDirectories)
                .Where(file => {
                    bool isHiddenFile = new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden);
                    bool isOutputFile = file.StartsWith(DefaultOutputDirectory);
                    return !(isHiddenFile || isOutputFile);
                 })
                .ToList();

            var generator = new ProjectGenerator();
            generator.Generate(source, files, destination);

            return 0;
        }
    }
}
