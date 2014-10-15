using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> WhereMissing<T, T1>(this IQueryable<T> source, Expression<Func<T, T1>> predicate)
        {
            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    ((MethodInfo)MethodBase.GetCurrentMethod())
                        .MakeGenericMethod(typeof(T), typeof(T1)),
                    source.Expression,
                    Expression.Quote(predicate)));

        }



    }
}
