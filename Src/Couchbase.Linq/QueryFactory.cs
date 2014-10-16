using Couchbase.Core;

namespace Couchbase.Linq
{
    public class QueryFactory
    {
        public static BucketQueryable<T> Queryable<T>(IBucket bucket)
        {
            return new BucketQueryable<T>(bucket);
        } 
    }
}
