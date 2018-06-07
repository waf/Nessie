using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Nessie.Services.Models
{
    public class ProcessOutput
    {
        public ProcessOutput(string output, ImmutableDictionary<string, object> variables)
        {
            Output = output;
            Variables = variables;
        }

        public string Output { get; }

        /// <summary>
        /// Variables that the processing of this file has produced.
        /// Variables are produced by evaluating Liquid templates.
        /// </summary>
        public ImmutableDictionary<string, object> Variables { get; }

        public override bool Equals(object obj)
        {
            var output = obj as ProcessOutput;
            return output != null &&
                   Output == output.Output &&
                   EqualityComparer<ImmutableDictionary<string, object>>.Default.Equals(Variables, output.Variables);
        }

        public override int GetHashCode()
        {
            var hashCode = 1841734950;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Output);
            hashCode = hashCode * -1521134295 + EqualityComparer<ImmutableDictionary<string, object>>.Default.GetHashCode(Variables);
            return hashCode;
        }
    }
}
