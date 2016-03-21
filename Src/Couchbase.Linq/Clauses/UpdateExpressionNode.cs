using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;

namespace Couchbase.Linq.Clauses
{
    internal class UpdateExpressionNode : MethodCallExpressionNodeBase
    {
        // see reference source https://github.com/re-motion/Relinq/blob/fe7061590f2204cbe12477529df723d52de9047b/Core/Parsing/Structure/IntermediateModel/WhereExpressionNode.cs

        public static readonly MethodInfo[] SupportedMethods =
        {
            typeof (QueryExtensions).GetMethod(nameof(QueryExtensions.Set)),
            typeof (QueryExtensions).GetMethod(nameof(QueryExtensions.Unset)),
        };

        private readonly ResolvedExpressionCache<Expression> _cachedPredicate;
        bool unset;

        public UpdateExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression predicate)
            : base(parseInfo)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            unset = parseInfo.ParsedExpression.Method.Name == nameof(QueryExtensions.Unset);
            _predicate = predicate;
            _cachedPredicate = new ResolvedExpressionCache<Expression>(this);
        }

        public LambdaExpression _predicate { get; private set; }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        public Expression GetResolvedPredicate(ClauseGenerationContext clauseGenerationContext)
        {
            return _cachedPredicate.GetOrCreate(r => r.GetResolvedExpression(_predicate.Body, _predicate.Parameters[0], clauseGenerationContext));
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            var upd = new UpdateClause();
            if (unset)
                upd.Unsetters.Add(GetResolvedPredicate(clauseGenerationContext));
            else
                upd.Setters.Add(GetResolvedPredicate(clauseGenerationContext));

            queryModel.BodyClauses.Add(upd);
        }
    }
}