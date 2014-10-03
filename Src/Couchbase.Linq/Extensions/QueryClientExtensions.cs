using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Couchbase.N1QL;

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
