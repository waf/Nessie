using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nessie
{
    public class FileOutput
    {
        public FileOutput(FileLocation name, string output, ImmutableDictionary<string, object> variables)
        {
            this.Name = name;
            this.Variables = variables;
            this.Output = output;
        }

        public FileLocation Name { get; }
        public string Output { get; }
        public ImmutableDictionary<string, object> Variables { get; }

        public override bool Equals(object obj)
        {
            var output = obj as FileOutput;
            return output != null &&
                   EqualityComparer<FileLocation>.Default.Equals(Name, output.Name) &&
                   Output == output.Output &&
                   EqualityComparer<ImmutableDictionary<string, object>>.Default.Equals(Variables, output.Variables);
        }

        public override int GetHashCode()
        {
            var hashCode = 1744653196;
            hashCode = hashCode * -1521134295 + EqualityComparer<FileLocation>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Output);
            hashCode = hashCode * -1521134295 + EqualityComparer<ImmutableDictionary<string, object>>.Default.GetHashCode(Variables);
            return hashCode;
        }
    }
}
