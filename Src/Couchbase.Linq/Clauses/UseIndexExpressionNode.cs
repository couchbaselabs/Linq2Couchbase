using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    internal class UseIndexExpressionNode : MethodCallExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
            typeof(QueryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.Name == "UseIndex")
                .ToArray();

        public UseIndexExpressionNode(MethodCallExpressionParseInfo parseInfo, ConstantExpression indexName, ConstantExpression indexType)
            : base(parseInfo)
        {
            if (indexName == null)
            {
                throw new ArgumentNullException("indexName");
            }
            if (indexName.Type != typeof(string))
            {
                throw new ArgumentException("indexName must return a string", "indexName");
            }

            if (indexType.Type != typeof(N1QlIndexType))
            {
                throw new ArgumentException("indexType must return a N1QlIndexType", "indexType");
            }

            IndexName = indexName;
            IndexType = indexType;
        }

        public ConstantExpression IndexName { get; private set; }
        public ConstantExpression IndexType { get; private set; }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            queryModel.BodyClauses.Add(new UseIndexClause((string) IndexName.Value, (N1QlIndexType) IndexType.Value));
        }
    }
}