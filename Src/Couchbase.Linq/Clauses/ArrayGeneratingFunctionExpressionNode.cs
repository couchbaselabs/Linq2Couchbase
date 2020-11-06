using System;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Utilities;

namespace Couchbase.Linq.Clauses
{
    internal sealed class ArrayGeneratingFunctionExpressionNode : IQuerySourceExpressionNode
    {
        private readonly Type _querySourceElementType;
        public Type QuerySourceElementType => _querySourceElementType;

        public Type QuerySourceType { get; }
        public Expression ParsedExpression { get; }
        public string AssociatedIdentifier { get; }
        public IExpressionNode Source => null;

        public ArrayGeneratingFunctionExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression argument1)
        {
            QuerySourceType = parseInfo.ParsedExpression.Type;

            if (!ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable(parseInfo.ParsedExpression.Type,
                out _querySourceElementType))
            {
                _querySourceElementType = typeof(object);
            }

            ParsedExpression = parseInfo.ParsedExpression;
            AssociatedIdentifier = parseInfo.AssociatedIdentifier;
        }

        public Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            if (inputParameter == null)
            {
                throw new ArgumentNullException(nameof(inputParameter));
            }
            if (expressionToBeResolved == null)
            {
                throw new ArgumentNullException(nameof(expressionToBeResolved));
            }

            return QuerySourceExpressionNodeUtility.ReplaceParameterWithReference(
                this,
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);
        }

        public QueryModel Apply(QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
        {
            if (queryModel != null)
            {
                throw new ArgumentException(
                    "QueryModel has to be null because MainSourceExpressionNode marks the start of a query.",
                    nameof(queryModel));
            }

            var mainFromClause = CreateMainFromClause(clauseGenerationContext);
            var defaultSelectClause = new SelectClause(new QuerySourceReferenceExpression(mainFromClause));
            return new QueryModel (mainFromClause, defaultSelectClause) { ResultTypeOverride = QuerySourceType };
        }

        private MainFromClause CreateMainFromClause(ClauseGenerationContext clauseGenerationContext)
        {
            var fromClause = new MainFromClause (
                AssociatedIdentifier,
                QuerySourceElementType,
                ParsedExpression);

            clauseGenerationContext.AddContextInfo(this, fromClause);
            return fromClause;
        }
    }
}