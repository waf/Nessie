using Nessie.DevServer;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Nessie.Commands
{
    public sealed class ServeCommand
    {
        public int Run(string hostname, int port, bool browse)
        {
            string url = $"http://{hostname}:{port}";
            string serverRoot = Path.Combine(
                ProjectLocator.FindProjectDirectory(),
                ProjectLocator.OutputDirectory
            );
            Directory.CreateDirectory(serverRoot);

            using (var server = new HttpServer(serverRoot, hostname, port))
            {
                server.Start();

                SetupAutoBuildRefresh(server);

                if (browse)
                {
                    RunWebBrowser(url);
                }

                Console.WriteLine("Listening at " + url);
                Console.WriteLine("Press any key to quit.");
                Console.WriteLine();
                Console.ReadKey();
            }
            return 0;
        }

        private static void SetupAutoBuildRefresh(HttpServer server)
        {
            var build = new BuildCommand();
            build.Run(watch: true, silent: true);
            build.OnBuilt += (sender, e) => server.SendClientRefresh();
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
    }
}