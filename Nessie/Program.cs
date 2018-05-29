using McMaster.Extensions.CommandLineUtils;
using Nessie.Commands;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nessie
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var app = DefineOptions();
            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException parseException)
            {
                app.ShowHelp();
                app.Error.WriteLine(parseException.Message);
                app.Error.WriteLine();
            }
        }

        public static CommandLineApplication DefineOptions()
        {
            var app = new CommandLineApplication();
            app.Name = "nessie";
            app.FullName = "Nessie";
            app.VersionOption("-v|--version", "v0.0.1");
            app.HelpOption();

            app.Command("build", build =>
            {
                build.Description = "Generates the static site";
                build.HelpOption();

                var watch = build.Option("-w|--watch", "Watch the filesystem for changes and rebuild", CommandOptionType.NoValue);
                build.OnExecute(() =>
                {
                    new BuildCommand().Run(
                        watch.GetValueOrDefault(false),
                        silent: false
                    );
                    return 0;
                });
            });

            app.Command("serve", serve =>
            {
                serve.Description = "Runs a local development HTTP server";
                serve.HelpOption();

                var hostname = serve.Option("-n|--hostname", "The hostname to use for the http server. Defaults to localhost", CommandOptionType.SingleValue);
                var port = serve.Option("-p|--port", "The port to use for the http server. Defaults to 8080", CommandOptionType.SingleValue);
                var noBrowse = serve.Option("-nb|--no-browse", "Don't launch the system default browser.", CommandOptionType.NoValue);
                serve.OnExecute(() =>
                {
                    new ServeCommand().Run(
                        hostname.GetValueOrDefault("localhost"),
                        port.GetValueOrDefault(8080),
                        browse: !noBrowse.GetValueOrDefault(true)
                    );
                    return 0;
                });
            });

            // when no commands are supplied
            app.OnExecute(() => app.ShowHelp());

            return app;
        }
    }
}