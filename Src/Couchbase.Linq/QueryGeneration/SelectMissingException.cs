using System;
using System.Runtime.Serialization;

namespace Couchbase.Linq.QueryGeneration
{
    public sealed class SelectMissingException : InvalidOperationException
    {
        public SelectMissingException()
        {
        }

        public SelectMissingException(string message)
            : base(message)
        {
        }

        public SelectMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SelectMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}