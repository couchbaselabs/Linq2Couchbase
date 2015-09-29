using System.Linq;
using Couchbase.Core;

namespace Couchbase.Linq
{
    internal class QueryFactory
    {
        public static IQueryable<T> Queryable<T>(IBucket bucket)
        {
            return new BucketQueryable<T>(bucket);
        }
    }
}