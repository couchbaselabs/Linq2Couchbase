using Couchbase.N1QL;
using System;

namespace Couchbase.Linq.Extensions
{
    public static class QueryClientExtensions
    {
        public static QueryClientQueryable<T> Queryable<T>(this IQueryClient queryClient, string bucketName, Uri uri)
        {
            return new QueryClientQueryable<T>(queryClient, bucketName, uri);
        }
    }
}
