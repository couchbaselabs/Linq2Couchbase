using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    internal class UseKeysExpressionNode : MethodCallExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            typeof (QueryExtensions).GetMethod("UseKeys")!
        };

        public UseKeysExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression keys)
            : base(parseInfo)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            Keys = keys;
        }

        public Expression Keys { get; private set; }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            queryModel.BodyClauses.Add(new UseKeysClause(Keys));
        }
    }
}