using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    internal class ExecuteExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            typeof (QueryExtensions).GetMethod(nameof(QueryExtensions.Execute)),
        };


        public ExecuteExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate)
            : base (parseInfo, optionalPredicate, null)
        {
        }


        public override Expression Resolve(
            ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            throw CreateResolveNotSupportedException();
        }


        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new ExecuteResultOperator();
        }
    }
}