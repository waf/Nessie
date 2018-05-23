using Microsoft.Extensions.DependencyInjection;
using Nessie.Commands;
using Nessie.Services;
using Nessie.Services.Converters;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nessie
{
    public static class Program
    {
        static Type[] commandTypes =
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

        static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            commandTypes.ForEach(cmd => services.AddTransient(cmd));
            services.AddTransient<ProjectGenerator>();
            services.AddTransient<FileGenerator>();
            services.AddTransient<TemplateConverter>();
            services.AddTransient<MarkdownConverter>();
            services.AddTransient<ReadFromFile>(svc => File.ReadAllText);
            services.AddTransient<WriteToFile>(svc => File.WriteAllText);
            return services.BuildServiceProvider();
        }

        static string ParseArguments(ICommand[] commands, string[] args)
        {
            string chosenCommandName = null;

            ArgumentSyntax.Parse(args, syntax =>
            {
                commands.ForEach(command =>
                {
                    command.DefineArguments(syntax, ref chosenCommandName);
                });
            });

            return chosenCommandName;
        }
    }
}