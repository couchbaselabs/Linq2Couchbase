using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq.Views
{
    public class ViewQueryable<T> : QueryableBase<T>
    {
        public ViewQueryable(IQueryParser queryParser, IQueryExecutor executor) 
            : base(queryParser, executor)
        {
        }

        public ViewQueryable(IQueryProvider provider) 
            : base(provider)
        {
        }

        public ViewQueryable(IQueryProvider provider, Expression expression) 
            : base(provider, expression)
        {
        }
    }
}
