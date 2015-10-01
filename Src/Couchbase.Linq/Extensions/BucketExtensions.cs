using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq.Extensions
{
    public static class BucketExtensions
    {
        internal static IQueryable<T> Queryable<T>(this IBucket bucket)
        {
            //TODO refactor so ClientConfiguration is injectable
            return EntityFilterManager.ApplyFilters(new BucketQueryable<T>(bucket, new ClientConfiguration()));
        }
    }
}