using System;
using Couchbase.N1QL;

namespace Couchbase.Linq.Extensions
{
    public static class QueryClientExtensions
    {
        internal static QueryClientQueryable<T> Queryable<T>(this IQueryClient queryClient, string bucketName, Uri uri)
        {
            return new QueryClientQueryable<T>(queryClient, bucketName, uri);
        }
    }
}