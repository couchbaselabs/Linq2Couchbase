using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Operators;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    /// <summary>
    /// Expression node for FirstAsync and FirstOrDefaultAsync.
    /// </summary>
    internal class FirstAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.FirstAsyncNoPredicate,
            QueryExtensionMethods.FirstAsyncWithPredicate,
            QueryExtensionMethods.FirstOrDefaultAsyncNoPredicate,
            QueryExtensionMethods.FirstOrDefaultAsyncWithPredicate
        };

        public FirstAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate)
            : base(parseInfo, optionalPredicate, null)
        {
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(FirstAsyncExpressionNode)}");
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new FirstAsyncResultOperator(ParsedExpression.Method.Name.EndsWith("OrDefaultAsync"));
    }
}
