using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    public class WhereMissingExpressionNode : MethodCallExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            GetSupportedMethod(() => QueryExtensions.WhereMissing<object, object>(null, o => null)),
            GetSupportedMethod(() => QueryExtensions.WhereMissing<object, int>(null, o => 10))
        };

        private readonly ResolvedExpressionCache<Expression> _cachedPredicate;

        public WhereMissingExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression predicate)
            : base(parseInfo)
        {
            if (predicate.Parameters.Count != 1)
                throw new ArgumentException("Predicate must have exactly one parameter.", "predicate");

            Predicate = predicate;
            _cachedPredicate = new ResolvedExpressionCache<Expression>(this);
        }

        public LambdaExpression Predicate { get; private set; }

        public Expression GetResolvedPredicate(ClauseGenerationContext clauseGenerationContext)
        {
            return
                _cachedPredicate.GetOrCreate(
                    r => r.GetResolvedExpression(Predicate.Body, Predicate.Parameters[0], clauseGenerationContext));
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override QueryModel ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            queryModel.BodyClauses.Add(new WhereMissingClause(GetResolvedPredicate(clauseGenerationContext)));

            return queryModel;
        }
    }
}