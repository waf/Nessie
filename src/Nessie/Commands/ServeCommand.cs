using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.CommandLine;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Nessie.Commands
{
    class ServeCommand : ICommand
    {
        public string Name => "serve";

        int port;
        bool browse;
        bool watch;

        public void DefineArguments(ArgumentSyntax syntax, ref string command)
        {
            syntax.DefineCommand(Name, ref command, "runs a local development HTTP server");
            port = syntax.DefineOption("p|port", 8080, "the port to use for the http server. Defaults to 8080").Value;
            watch = syntax.DefineOption("w|watch", false, "watch the filesystem for changes and rebuild").Value;
            browse = syntax.DefineOption("b|browse", false, "launch the system default browser.").Value;
        }

        public int Run()
        {
            string url = "http://localhost:" + port;
            using (var server = RunWebServer(url))
            {
                server.Start();
                new BuildCommand().Run();

                if (browse)
                {
                    Console.WriteLine("Launch system browser");
                    RunWebBrowser(url);
                }
                if (watch)
                {
                    Console.WriteLine("Watching for file changes");
                    RunFileWatcher();
                }

                Console.WriteLine("Listening at " + url);
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
            }
            return 0;
        }

        private void RunWebBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")); // Works ok on windows
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else // linux, etc
            {
                Process.Start("xdg-open", url);
            }
        }

        private IWebHost RunWebServer(string url)
        {
            string serverRoot = Path.Combine(Directory.GetCurrentDirectory(), BuildCommand.DefaultOutputDirectory);
            Directory.CreateDirectory(serverRoot);
            return new WebHostBuilder()
                .UseContentRoot(serverRoot)
                .Configure(app =>
                {
                    app.UseFileServer(new FileServerOptions
                    {
                        FileProvider = new PhysicalFileProvider(serverRoot),
                        EnableDefaultFiles = false,
                        EnableDirectoryBrowsing = true
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddDirectoryBrowser();
                })
                .UseKestrel()
                .UseUrls(url)
                .Build();
        }

        private void RunFileWatcher()
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
                    new BuildCommand().Run();
                }

                watcher.EnableRaisingEvents = true;
            };
            watcher.EnableRaisingEvents = true;
        }

        private bool IsInOutputDirectory(string filepath)
        {
            return filepath.StartsWith(Path.GetFullPath(BuildCommand.DefaultOutputDirectory), StringComparison.CurrentCulture);
        }
    }
}