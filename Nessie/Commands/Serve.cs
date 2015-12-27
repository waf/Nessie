using ManyConsole;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Commands
{
    class Serve : ConsoleCommand
    {
        // command line arguments with defaults
        private int port = 8080;
        private bool browse = false;
        private bool watch = false;

        public Serve()
        {
            IsCommand("serve", "runs a local development HTTP server");

            HasOption("p|port=", $"the port to use for the http server.\ndefaults to {port}",
                input => port = int.Parse(input));
            HasOption("b|browse", $"launch the system default browser.",
                input => browse = true);
            HasOption("w|watch", $"watch the filesystem for changes and rebuild",
                input => watch = true);
        }

        public override int Run(string[] remainingArguments)
        {
            string url = "http://localhost:" + port;

            using (var server = WebApp.Start(url, ConfigureHttpServer))
            {
                if(browse)
                {
                    Process.Start(url);
                }
                if(watch)
                {
                    var watcher = ConfigureFileWatcher();
                    watcher.EnableRaisingEvents = true;
                }
                Console.WriteLine("Listening at " + url);
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
            }
            return 0;
        }

        private void ConfigureHttpServer(IAppBuilder builder)
        {
            string serverRoot = Path.Combine(Directory.GetCurrentDirectory(), Build.DefaultOutputDirectory);
            Directory.CreateDirectory(serverRoot);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = new PhysicalFileSystem(serverRoot)
            };
            builder.UseFileServer(options);
        }

        private FileSystemWatcher ConfigureFileWatcher()
        {
            var watcher = new FileSystemWatcher();
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Path = Directory.GetCurrentDirectory();
            watcher.Changed += (sender, e) => {
                watcher.EnableRaisingEvents = false;

                var fileAttrs = new FileInfo(e.FullPath).Attributes;
                if (!(IsInOutputDirectory(e.FullPath) && 
                      fileAttrs.HasFlag(FileAttributes.Directory) && 
                      fileAttrs.HasFlag(FileAttributes.Hidden)))
                {
                    Console.WriteLine("\nChange detected, regenerating files");
                    new Build().Run(null);
                }

                watcher.EnableRaisingEvents = true;
            };
            return watcher;
        }

        private bool IsInOutputDirectory(string filepath)
        {
            return filepath.StartsWith(Path.GetFullPath(Build.DefaultOutputDirectory));
        }
    }
}
