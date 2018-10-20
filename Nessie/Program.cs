using McMaster.Extensions.CommandLineUtils;
using Nessie.Commands;
using Nessie.Services.Processors;

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
            catch (ErrorMessageException errorMessage)
            {
                app.Error.WriteLine(errorMessage.Message);
            }
        }

        public static CommandLineApplication DefineOptions()
        {
            var app = new CommandLineApplication
            {
                Name = "nessie",
                FullName = "Nessie"
            };
            app.OnExecute(() => app.ShowHelp()); // when no commands are supplied
            app.VersionOption("-v|--version", "v0.0.1");
            app.HelpOption();

            AddBuildCommand(app);
            AddServeCommand(app);

            return app;
        }

        private static void AddBuildCommand(CommandLineApplication app)
        {
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
        }

        private static void AddServeCommand(CommandLineApplication app)
        {
            const string DefaultHostName = "localhost";
            const int DefaultPort = 8080;

            app.Command("serve", serve =>
            {
                serve.Description = "Runs a local development HTTP server";
                serve.HelpOption();

                var hostname = serve.Option("-n|--hostname", $"The hostname to use for the http server. Defaults to {DefaultHostName}", CommandOptionType.SingleValue);
                var port = serve.Option("-p|--port", $"The port to use for the http server. Defaults to {DefaultPort}", CommandOptionType.SingleValue);
                var noBrowse = serve.Option("-nb|--no-browse", "Don't launch the system default browser.", CommandOptionType.NoValue);
                serve.OnExecute(() =>
                {
                    new ServeCommand().Run(
                        hostname.GetValueOrDefault(DefaultHostName),
                        port.GetValueOrDefault(DefaultPort),
                        browse: !noBrowse.GetValueOrDefault(false)
                    );
                    return 0;
                });
            });
        }
    }
}