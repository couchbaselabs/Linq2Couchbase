using System;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;

namespace Couchbase.Linq.Clauses
{
    public class WhereMissingClause : IBodyClause
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WhereMissingClause" /> class.
        /// </summary>
        /// <param name="predicate">The predicate used to filter data items.</param>
        public WhereMissingClause(Expression predicate)
        {
            Predicate = predicate;
        }

        /// <summary>
        ///     Gets the predicate, the expression representing the where condition by which the data items are filtered
        /// </summary>
        public Expression Predicate { get; set; }

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
            var visotorx = visitor as N1QlQueryModelVisitor;
            if (visotorx != null) visotorx.VisitWhereMissingClause(this, queryModel, index);
        }

        /// <summary>
        ///     Transforms all the expressions in this clause and its child objects via the given
        ///     <paramref name="transformation" /> delegate.
        /// </summary>
        /// <param name="transformation">
        ///     The transformation object. This delegate is called for each <see cref="Expression" /> within this
        ///     clause, and those expressions will be replaced with what the delegate returns.
        /// </param>
        public void TransformExpressions(Func<Expression, Expression> transformation)
        {
            Predicate = transformation(Predicate);
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
        public virtual WhereMissingClause Clone(CloneContext cloneContext)
        {
            var clone = new WhereMissingClause(Predicate);
            return clone;
        }

        public override string ToString()
        {
            return "WHERE MISSING " + FormattingExpressionTreeVisitor.Format(Predicate);
        }
    }
}