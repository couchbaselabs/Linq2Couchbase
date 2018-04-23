using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace Couchbase.Linq.UnitTests
{
    internal class QueryFactory
    {
        public static IQueryable<T> Queryable<T>(IBucket bucket)
        {
            //TODO refactor so ClientConfiguration is injectable
            return new BucketQueryable<T>(bucket, new ClientConfiguration(), new BucketContext(bucket));
        }
    }
}