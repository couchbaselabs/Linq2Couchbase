using System.Linq.Expressions;
using System.Text;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Expression tree visitor which builds a N1QL expression from the tree.
    /// </summary>
    public interface IN1QlExpressionTreeVisitor
    {
        /// <summary>
        /// N1QL query being built.
        /// </summary>
        StringBuilder Expression { get; }

        /// <summary>
        /// Visits a node in the expression tree and any child nodes, rendering
        /// them onto the <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">Node to visit.</param>
        void Visit(Expression expression);
    }
}