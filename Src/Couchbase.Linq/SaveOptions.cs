using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides options controlling how changes are saved via <see cref="IBucketContext"/>.
    /// </summary>
    public class SaveOptions
    {
        /// <summary>
        /// If true, before saving documents the Couchbase bucket is checked to ensure that another process
        /// has not modified the document since it was read.  If it was modified, a
        /// <see cref="CouchbaseConsistencyException"/> is thrown.  For new documents, the exception will be
        /// thrown if the document already exists.  Defaults to true.
        /// </summary>
        public bool PerformConsistencyCheck { get; set; }

        /// <summary>
        /// Creates a new <see cref="SaveOptions"/> object.
        /// </summary>
        public SaveOptions()
        {
            PerformConsistencyCheck = true;
        }
    }
}
