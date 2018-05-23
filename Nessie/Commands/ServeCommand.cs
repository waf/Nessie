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

        public void DefineArguments(ArgumentSyntax syntax, ref string command)
        {
            syntax.DefineCommand(Name, ref command, "runs a local development HTTP server");
            port = syntax.DefineOption("p|port", 8080, "the port to use for the http server. Defaults to 8080").Value;
            browse = syntax.DefineOption("b|browse", false, "launch the system default browser.").Value;
        }

        public int Run()
        {
            string url = "http://localhost:" + port;
            using (var server = RunWebServer(url))
            {
                server.Start();
                Console.WriteLine("Listening at " + url);

                if (browse)
                {
                    RunWebBrowser(url);
                }

                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
            }
            return 0;
        }

        private void RunWebBrowser(string url)
        {
            Console.WriteLine("Launching system browser");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
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
            string serverRoot = Path.Combine(Directory.GetCurrentDirectory(), BuildCommand.OutputDirectory);
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
    }
}