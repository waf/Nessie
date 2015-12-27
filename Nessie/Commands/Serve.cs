using ManyConsole;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Commands
{
    class Serve : ConsoleCommand
    {
        private string destination = Build.DefaultOutputDirectory;
        private int port = 8080;

        public Serve()
        {
            IsCommand("serve", "runs a local development HTTP server");

            HasOption("p|port=", $"the port to use for the http server. defaults to {port}",
                input => port = int.Parse(input));
            HasOption("d|dest=", $"the destination directory to serve as the server root. defaults to '{Build.DefaultOutputDirectory}'",
                input => destination = input);
        }

        public override int Run(string[] remainingArguments)
        {
            string url = "http://localhost:" + port;

            using (var server = WebApp.Start(url, ConfigureServer))
            {
                Console.WriteLine("Listening at " + url);
                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }
            return 0;
        }

        private void ConfigureServer(IAppBuilder builder)
        {
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = new PhysicalFileSystem(Path.Combine(Directory.GetCurrentDirectory(), destination))
            };
            builder.UseFileServer(options);
        }
    }
}
