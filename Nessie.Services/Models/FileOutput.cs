using Nessie.Services.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nessie.Services.Models
{
    public class FileOutput : ProcessOutput
    {
        public FileOutput(FileLocation name, string output, ImmutableDictionary<string, object> variables)
            : base(output, variables)
        {
            this.Name = name;
        }

        public FileLocation Name { get; }

        public override bool Equals(object obj)
        {
            var output = obj as FileOutput;
            return output != null &&
                   base.Equals(obj) &&
                   EqualityComparer<FileLocation>.Default.Equals(Name, output.Name);
        }

        public override int GetHashCode()
        {
            var hashCode = 890389916;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FileLocation>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
