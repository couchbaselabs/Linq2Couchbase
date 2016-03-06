using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.N1QL;

namespace Couchbase.Linq
{
    /// <summary>
    /// Thrown if an error occurs during the execution of a LINQ N1QL query.
    /// </summary>
    public class CouchbaseQueryException : ApplicationException
    {
        private readonly ReadOnlyCollection<Error> _errors;

        /// <summary>
        /// Errors returned by the Couchbase Server, if any.
        /// </summary>
        public ReadOnlyCollection<Error> Errors
        {
            get { return _errors; }
        }

        internal CouchbaseQueryException(string message) :
            this(message, (Exception) null)
        {
        }

        internal CouchbaseQueryException(string message, Exception innerException) :
            base(message, innerException)
        {
            _errors = new ReadOnlyCollection<Error>(new Error[] {});
        }

        internal CouchbaseQueryException(string message, IList<Error> errors) :
            base(message)
        {
            _errors = new ReadOnlyCollection<Error>(errors);
        }
    }
}
