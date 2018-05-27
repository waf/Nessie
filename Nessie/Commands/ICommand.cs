using System.CommandLine;

namespace Nessie.Commands
{
    interface ICommand
    {
        string Name { get; }
        void DefineArguments(ArgumentSyntax syntax, ref string command);
        int Run();
    }
}
