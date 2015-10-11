using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Transforms references to the Key property of an IGrouping into another expression.
    /// Used to convert references to the Key of a GroupBy statement to directly access the property
    /// used to make the key.  This is done after a grouping subquery is flattened into the main N1QL query.
    /// </summary>
    internal class KeyExpressionTransfomer : IExpressionTransformer<MemberExpression>
    {
        public ExpressionType[] SupportedExpressionTypes
        {
            get
            {
                return new[]
                {
                    ExpressionType.MemberAccess
                };
            }
        }

        private readonly QuerySourceReferenceExpression _querySourceReference;
        private readonly PropertyInfo _keyPropertyInfo;
        private readonly Expression _replacementExpression;

        /// <summary>
        /// Creates a new KeyExpressionTransformer
        /// </summary>
        /// <param name="querySourceReference">QuerySourceReferenceExpression that references an IQuerySource returning an IGrouping</param>
        /// <param name="replacementExpression">Expression to replace any reference to the Key property of the IGrouping</param>
        public KeyExpressionTransfomer(QuerySourceReferenceExpression querySourceReference, Expression replacementExpression)
        {
            if (querySourceReference == null)
            {
                throw new ArgumentNullException("querySourceReference");
            }
            if (replacementExpression == null)
            {
                throw new ArgumentNullException("replacementExpression");
            }

            _querySourceReference = querySourceReference;
            _keyPropertyInfo = querySourceReference.ReferencedQuerySource.ItemType.GetProperty("Key");
            _replacementExpression = replacementExpression;
        }

        public Expression Transform(MemberExpression expression)
        {
            if (expression.Expression.Equals(_querySourceReference) && (expression.Member == _keyPropertyInfo))
            {
                return _replacementExpression;
            }

            return expression;
        }

    }
}
