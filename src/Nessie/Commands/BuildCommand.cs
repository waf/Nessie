using Nessie.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.Threading;

namespace Nessie.Commands
{
    class BuildCommand : ICommand
    {
        public string Name => "build";
        public const string OutputDirectory = "_output";

        // command line parameters
        bool watch;

        // dependencies
        readonly ProjectGenerator projectGenerator;

        public BuildCommand(ProjectGenerator projectGenerator)
        {
            this.projectGenerator = projectGenerator;
        }

        public void DefineArguments(ArgumentSyntax syntax, ref string command)
        {
            syntax.DefineCommand(Name, ref command, "generates the static site");
            watch = syntax.DefineOption("w|watch", false, "watch the filesystem for changes and rebuild").Value;
        }

        public int Run()
        {
            string projectDirectory = FindProjectDirectory(Path.GetFullPath("."));

            Build(projectDirectory);

            if (watch)
            {
                RunFileWatcher(projectDirectory);
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
            }

            return 0;
        }

        private void Build(string projectDirectory)
        {
            var files = Directory
                .GetFiles(projectDirectory, "*", SearchOption.AllDirectories)
                .Where(IsValidInputFile)
                .ToList();

            projectGenerator.Generate(projectDirectory, files, Path.Combine(projectDirectory, OutputDirectory));

            string time = DateTime.Now.ToString("T");
            Console.WriteLine($"{time}: Built site");
        }

        private string FindProjectDirectory(string originalPath)
        {
            return FindProjectDirectory(originalPath, originalPath);
        }

        private string FindProjectDirectory(string originalPath, string thisPath)
        {
            if(Directory.GetDirectories(thisPath).Contains(OutputDirectory))
            {
                return thisPath;
            }
            string parent = Directory.GetParent(thisPath)?.FullName;
            bool atRootDirectory = parent == null;
            if(atRootDirectory)
            {
                return originalPath;
            }
            return FindProjectDirectory(originalPath, parent);
        }

        private static bool IsValidInputFile(string file)
        {
            bool isHiddenFile = new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden);
            bool isOutputFile = file.Split(Path.DirectorySeparatorChar).Any(part => part == OutputDirectory);
            return !(isHiddenFile || isOutputFile);
        }

        private void RunFileWatcher(string projectDirectory)
        {
            Console.WriteLine("Watching for file changes");
            var watcher = new FileSystemWatcher();
            watcher.Path = projectDirectory;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Changed += (sender, e) => {
                watcher.EnableRaisingEvents = false;

                if (IsValidInputFile(e.FullPath) &&
                    !new FileInfo(e.FullPath).Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine("\nChange detected, regenerating files");
                    Build(projectDirectory);
                }

                watcher.EnableRaisingEvents = true;
            };
            watcher.EnableRaisingEvents = true;
        }
    }
}