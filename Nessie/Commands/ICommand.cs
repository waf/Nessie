using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Nessie.Commands
{
    interface ICommand
    {
        string Name { get; }
        void DefineArguments(ArgumentSyntax syntax, ref string command);
        int Run();
    }
}
