using Nessie.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Nessie
{
    public static class Program
    {
        static ICommand[] commands =
        {
            new BuildCommand(),
            new ServeCommand()
        };

        public static void Main(string[] args)
        {
            string chosenCommandName = null;

            ArgumentSyntax.Parse(args, syntax =>
            {
                commands.ForEach(command =>
                {
                    command.DefineArguments(syntax, ref chosenCommandName);
                });
            });

            var commandsByName = commands.ToDictionary(command => command.Name);
            commandsByName[chosenCommandName].Run();
        }
    }
}