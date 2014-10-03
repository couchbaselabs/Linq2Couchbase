using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
