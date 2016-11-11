using System;

namespace Couchbase.Linq
{
    /// <summary>
    /// Thrown if a write operation fails because the document has been modified since it was read.
    /// </summary>
    public class CouchbaseConsistencyException : CouchbaseWriteException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CouchbaseConsistencyException"/> class.
        /// </summary>
        /// <param name="failedResult">The <see cref="IOperationResult"/> of the failed operation.</param>
        public CouchbaseConsistencyException(IOperationResult failedResult) : base(failedResult)
        {
        }
    }
}
