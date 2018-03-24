using System;
using System.Linq.Expressions;
using System.Text;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    internal abstract class HintClause : IBodyClause
    {
        /// <summary>
        ///     Accepts the specified visitor
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="queryModel">The query model in whose context this clause is visited.</param>
        /// <param name="index">
        ///     The index of this clause in the <paramref name="queryModel" />'s
        ///     <see cref="QueryModel.BodyClauses" /> collection.
        /// </param>
        public virtual void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
        {
            if (visitor is IN1QlQueryModelVisitor visitorx)
            {
                visitorx.VisitHintClause(this, queryModel, index);
            }
        }

        /// <summary>
        ///     Transforms all the expressions in this clause and its child objects via the given
        ///     <paramref name="transformation" /> delegate.
        /// </summary>
        /// <param name="transformation">
        ///     The transformation object. This delegate is called for each <see cref="Expression" /> within this
        ///     clause, and those expressions will be replaced with what the delegate returns.
        /// </param>
        public virtual void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        IBodyClause IBodyClause.Clone(CloneContext cloneContext)
        {
            return Clone(cloneContext);
        }

        /// <summary>
        ///     Clones this clause.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns></returns>
        public abstract HintClause Clone(CloneContext cloneContext);

        /// <summary>
        ///     Appends the hint to the query <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        public abstract void AppendToStringBuilder(StringBuilder sb);

        public override string ToString()
        {
            return "use hint";
        }
    }
}
