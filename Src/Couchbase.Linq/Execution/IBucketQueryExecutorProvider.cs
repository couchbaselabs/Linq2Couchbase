using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Provides access to an <see cref="IBucketQueryExecutor"/>.
    /// </summary>
    internal interface IBucketQueryExecutorProvider
    {
        /// <summary>
        /// Get the <see cref="IBucketQueryExecutor"/>.
        /// </summary>
        IBucketQueryExecutor BucketQueryExecutor { get; }
    }
}
