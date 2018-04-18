using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    internal class UseHashExpressionNode : MethodCallExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
            typeof(QueryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.Name == "UseHash")
                .ToArray();

        public UseHashExpressionNode(MethodCallExpressionParseInfo parseInfo, ConstantExpression hashHintType)
            : base(parseInfo)
        {
            if (hashHintType.Type != typeof(HashHintType))
            {
                throw new ArgumentException($"{nameof(hashHintType)} must return a {nameof(Linq.HashHintType)}", nameof(hashHintType));
            }

            HashHintType = hashHintType;
        }

        public ConstantExpression HashHintType { get; }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            queryModel.BodyClauses.Add(new UseHashClause((HashHintType) HashHintType.Value));
        }
    }
}