using McMaster.Extensions.CommandLineUtils;
using Nessie.Commands;
using Nessie.Services.Processors;
using System.Reflection;

namespace Nessie
{
    /// <summary>
    /// Main entry point, defines command line options.
    /// </summary>
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
                PrintUserError(app, errorMessage);
            }
        }

        public static CommandLineApplication DefineOptions()
        {
            var app = new CommandLineApplication
            {
                Name = "nessie",
                FullName = "Nessie"
            };
            app.HelpOption();
            app.OnExecute(() => app.ShowHelp()); // when no commands are supplied

            // don't use app.VersionOptionFromAssemblyAttributes because it does not configure the -v flag.
            app.VersionOption("-v|--version", () => Version);

            AddBuildCommand(app);
            AddServeCommand(app);

            return app;
        }

        /// <summary>
        /// Build command builds the static site
        /// </summary>
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

        /// <summary>
        /// Serve command starts up a dev server
        /// </summary>
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

        /// <summary>
        /// Nicely format an error we want to present to the user.
        /// </summary>
        private static void PrintUserError(CommandLineApplication app, ErrorMessageException errorMessage)
        {
            app.Error.WriteLine(errorMessage.Message);
            foreach (var line in errorMessage.Data.Keys)
            {
                var value = errorMessage.Data[line]?.ToString();
                if (string.IsNullOrEmpty(value))
                    continue;

                app.Error.WriteLine(line.ToString());
                app.Error.WriteLine("  " + value);
            }
        }

        /// <summary>
        /// Read the value of the Version tag from the csproj.
        /// The AppVeyor build process will set this tag's value.
        /// </summary>
        private static string Version =>
            typeof(Program)
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
    }
}