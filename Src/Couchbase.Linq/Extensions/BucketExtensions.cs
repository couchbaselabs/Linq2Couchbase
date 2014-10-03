using Couchbase.Core;

namespace Couchbase.Linq.Extensions
{
    public static class BucketExtensions
    {
        public static BucketQueryable<T> Queryable<T>(this IBucket bucket)
        {
            return new BucketQueryable<T>(bucket);
        }
    }
}
