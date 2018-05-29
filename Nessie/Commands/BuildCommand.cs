using Nessie.Services;
using System;
using System.IO;
using System.Linq;

namespace Nessie.Commands
{
    sealed class BuildCommand
    {
        private readonly ProjectGenerator projectGenerator;
        private FileSystemWatcher watcher;

        /// <summary>
        /// Fired whenever a build is complete
        /// </summary>
        public event EventHandler OnBuilt;

        public BuildCommand() : this(new ProjectGenerator()) { }
        
        public BuildCommand(ProjectGenerator projectGenerator)
        {
            this.projectGenerator = projectGenerator;
        }

        /// <summary>
        /// Build the project
        /// </summary>
        /// <param name="watch">If true, watch for filesystem changes and rebuild</param>
        /// <param name="silent">If true, don't prompt for user input</param>
        /// <returns></returns>
        public int Run(bool watch, bool silent)
        {
            string projectDirectory = ProjectLocator.FindProjectDirectory();

            Build(projectDirectory);

            if (watch)
            {
                RunFileWatcher(projectDirectory);

                if(!silent)
                {
                    Console.WriteLine("Press any key to quit.");
                    Console.WriteLine();
                    Console.ReadKey();
                }
            }

            return 0;
        }

        private void Build(string projectDirectory)
        {
            var files = Directory
                .GetFiles(projectDirectory, "*", SearchOption.AllDirectories)
                .Where(IsValidInputFile)
                .ToList();

            projectGenerator.Generate(
                projectDirectory,
                files,
                Path.Combine(projectDirectory, ProjectLocator.OutputDirectory));

            string time = DateTime.Now.ToString("T");
            Console.WriteLine($"{time}: Built site");
            OnBuilt?.Invoke(this, EventArgs.Empty);
        }

        private static bool IsValidInputFile(string file)
        {
            bool isHiddenFile = new FileInfo(file).Attributes.HasFlag(FileAttributes.Hidden);
            bool isOutputFile = file
                .Split(Path.DirectorySeparatorChar)
                .Any(part => part == ProjectLocator.OutputDirectory);
            return !(isHiddenFile || isOutputFile);
        }

        private void RunFileWatcher(string projectDirectory)
        {
            Console.WriteLine("Watching for file changes");
            watcher = new FileSystemWatcher
            {
                Path = projectDirectory,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
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