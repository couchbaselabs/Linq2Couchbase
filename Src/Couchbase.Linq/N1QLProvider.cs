using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    public sealed class N1QlProvider : IQueryProvider
    {
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Query<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable) Activator.
                    CreateInstance(typeof (Query<>).
                    MakeGenericType(elementType),
                    new object[] {this, expression});
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult) Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        public string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
