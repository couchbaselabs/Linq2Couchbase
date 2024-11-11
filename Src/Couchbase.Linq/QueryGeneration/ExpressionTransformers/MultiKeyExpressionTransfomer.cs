using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Transforms references to the Key property of an IGrouping into another expression.
    /// Used to convert references to the Key of a GroupBy statement to directly access the properties
    /// used to make the key.  This is done after a grouping subquery is flattened into the main N1QL query.
    /// The MultiKeyExpressionTransformer variant is used for multipart keys, where accessing members of the Key property.
    /// </summary>
    internal class MultiKeyExpressionTransfomer : IExpressionTransformer<MemberExpression>
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
        private readonly NewExpression _newExpression;

        /// <summary>
        /// Creates a new KeyExpressionTransformer
        /// </summary>
        /// <param name="querySourceReference">QuerySourceReferenceExpression that references an IQuerySource returning an IGrouping</param>
        /// <param name="newExpression">NewExpression which was used to create the multipart key for grouping</param>
        public MultiKeyExpressionTransfomer(QuerySourceReferenceExpression querySourceReference, NewExpression newExpression)
        {
            if (querySourceReference == null)
            {
                throw new ArgumentNullException("querySourceReference");
            }
            if (newExpression == null)
            {
                throw new ArgumentNullException("newExpression");
            }

            _querySourceReference = querySourceReference;
            _keyPropertyInfo = querySourceReference.ReferencedQuerySource.ItemType.GetProperty("Key")!;
            _newExpression = newExpression;
        }

        public Expression Transform(MemberExpression expression)
        {
            var keyExpression = expression.Expression as MemberExpression;
            
            if ((keyExpression != null) && keyExpression.Expression!.Equals(_querySourceReference)
                && (keyExpression.Member == _keyPropertyInfo))
            {
                for (var i = 0; i < _newExpression.Members!.Count; i++)
                {
                    if (_newExpression.Members[i] == expression.Member)
                    {
                        return _newExpression.Arguments[i];
                    }
                }
            }

            return expression;
        }

    }
}
