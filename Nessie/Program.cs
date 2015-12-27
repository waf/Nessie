using ManyConsole;
using Nessie.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie
{
    class Program
    {
        static void Main(string[] args)
        {
            // scan this assembly for subclasses of ConsoleCommand, and register them as command-line options
            var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
            ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }
    }
}
