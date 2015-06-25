using System;
using System.Linq;
using Couchbase.Linq.Filters;
using Couchbase.N1QL;

namespace Couchbase.Linq.Extensions
{
    public static class QueryClientExtensions
    {
        internal static IQueryable<T> Queryable<T>(this IQueryClient queryClient, string bucketName, Uri uri)
        {
            return EntityFilterManager.ApplyFilters(new QueryClientQueryable<T>(queryClient, bucketName, uri));
        }
    }
}