using System;
using System.Runtime.Serialization;

namespace Nessie.Services.Processors
{
    [Serializable]
    public class ErrorMessageException : Exception
    {
        public ErrorMessageException() { }
        public ErrorMessageException(string message) : base(message) { }
        public ErrorMessageException(string message, Exception innerException) : base(message, innerException) { }
        protected ErrorMessageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}