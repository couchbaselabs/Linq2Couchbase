using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq;
using Couchbase.Linq.QueryGeneration;
using System.Collections.Generic;
using System;

namespace Couchbase.Linq.Clauses
{
    internal class UpdateClause : IBodyClause
    {
        public List<Expression> Setters { get; private set; } = new List<Expression>();
        public List<Expression> Unsetters { get; private set; } = new List<Expression>();
        
        public virtual void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
        {
            var visotorx = visitor as IN1QlQueryModelVisitor;
            if (visotorx != null)
                visotorx.VisitUpdateClause(this, queryModel, index);
        }
        
        public void TransformExpressions(Func<Expression, Expression> transformation)
        {
            throw new NotSupportedException("Update does not support transformations");
        }

        IBodyClause IBodyClause.Clone(CloneContext cloneContext)
        {
            return Clone(cloneContext);
        }


        public virtual UpdateClause Clone(CloneContext cloneContext)
        {
            var clone = new UpdateClause
            {
                Setters = new List<Expression>(Setters),
                Unsetters = new List<Expression>(Unsetters),
            };
            return clone;
        }

        public override string ToString()
        {
            return String.Format("set {0} unset {1}", string.Join(", ",Setters), string.Join(", ",Unsetters));
        }
    }
}