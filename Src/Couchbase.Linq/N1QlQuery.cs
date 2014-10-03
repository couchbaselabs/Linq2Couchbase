using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    public class Query<T> : IOrderedQueryable<T>
    {
        private readonly IQueryProvider _queryProvider;
        private readonly Expression _expression;

        public Query(IQueryProvider provider)
        {
            _queryProvider = provider;
            _expression = Expression.Constant(this);
        }

        public Query(IQueryProvider provider, Expression expression)
        {
            _queryProvider = provider;
            _expression = expression;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) Provider.Execute(_expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_queryProvider.Execute(_expression)).GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof (T); }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public IQueryProvider Provider
        {
            get { return _queryProvider; }
        }
    }
}
