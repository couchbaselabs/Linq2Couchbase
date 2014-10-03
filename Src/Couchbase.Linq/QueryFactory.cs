    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
