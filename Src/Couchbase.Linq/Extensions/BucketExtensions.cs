using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq.Extensions
{
    public static class BucketExtensions
    {
        public static IQueryable<T> Queryable<T>(this IBucket bucket)
        {
            return DocumentFilterManager.ApplyFilters(new BucketQueryable<T>(bucket));
        }
    }
}