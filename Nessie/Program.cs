using Microsoft.Extensions.DependencyInjection;
using Nessie.Commands;
using Nessie.Services;
using Nessie.Services.Processors;
using Nessie.Services.Utils;
using System;
using System.CommandLine;
using System.Linq;

namespace Nessie
{
    public static class Program
    {
        private static readonly Type[] commandTypes =
        {
            typeof(BuildCommand),
            typeof(ServeCommand)
        };

        public static void Main(string[] args)
        {
            // set up dependency injection
            var services = ConfigureServices();

            // resolve all commands, so we can use their argument definitions.
            var allCommands = commandTypes
                .Select(type => services.GetService(type) as ICommand)
                .ToArray();

            // parse and run the selected command.
            string chosenCommand = ParseArguments(allCommands, args);
            allCommands
                .SingleOrDefault(cmd => cmd.Name == chosenCommand)
                ?.Run();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            commandTypes.ForEach(cmd => services.AddTransient(cmd));
            services.AddTransient<ProjectGenerator>();
            services.AddTransient<FileGenerator>();
            services.AddTransient<TemplateService>();
            services.AddTransient<TemplateProcessor>();
            services.AddTransient<MarkdownProcessor>();
            services.AddTransient<FileOperation>();
            return services.BuildServiceProvider();
        }

        private static string ParseArguments(ICommand[] commands, string[] args)
        {
            string chosenCommandName = null;

            ArgumentSyntax.Parse(args, syntax =>
            {
                commands.ForEach(command =>
                    command.DefineArguments(syntax, ref chosenCommandName)
                );
            });

            return chosenCommandName;
        }
    }
}