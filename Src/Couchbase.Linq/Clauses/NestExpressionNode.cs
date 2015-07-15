using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    public class NestExpressionNode : MethodCallExpressionNodeBase, IQuerySourceExpressionNode
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            GetSupportedMethod(() => QueryExtensions.Nest<object, object, object>(null, null, o => null, (o, p) => null)),
            GetSupportedMethod(() => QueryExtensions.LeftOuterNest<object, object, object>(null, null, o => null, (o, p) => null))
        };

        private readonly ResolvedExpressionCache<Expression> _cachedKeySelector;
        private readonly ResolvedExpressionCache<Expression> _cachedResultSelector;

        public NestExpressionNode(MethodCallExpressionParseInfo parseInfo,
            Expression innerSequence,
            LambdaExpression keySelector,
            LambdaExpression resultSelector)
            : base(parseInfo)
        {
            if (innerSequence == null)
            {
                throw new ArgumentNullException("innerSequence");
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (keySelector.Parameters.Count != 1)
            {
                throw new ArgumentException("Key selector must have exactly one parameter.", "keySelector");
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }
            if (resultSelector.Parameters.Count != 2)
            {
                throw new ArgumentException("Result selector must have exactly two parameters.", "resultSelector");
            }

            InnerSequence = innerSequence;
            KeySelector = keySelector;
            ResultSelector = resultSelector;
            IsLeftOuterNest = parseInfo.ParsedExpression.Method.Name == "LeftOuterNest";

            _cachedKeySelector = new ResolvedExpressionCache<Expression>(this);
            _cachedResultSelector = new ResolvedExpressionCache<Expression>(this);
        }

        public Expression InnerSequence { get; private set; }
        public LambdaExpression KeySelector { get; private set; }
        public LambdaExpression ResultSelector { get; private set; }
        public bool IsLeftOuterNest { get; private set; }

        public Expression GetResolvedKeySelector(ClauseGenerationContext clauseGenerationContext)
        {
            return
                _cachedKeySelector.GetOrCreate(
                    r => r.GetResolvedExpression(KeySelector.Body, KeySelector.Parameters[0], clauseGenerationContext));
        }

        public Expression GetResolvedResultSelector(ClauseGenerationContext clauseGenerationContext)
        {
            return
                _cachedResultSelector.GetOrCreate(
                    r => r.GetResolvedExpression(
                        QuerySourceExpressionNodeUtility.ReplaceParameterWithReference(
                            this,
                            ResultSelector.Parameters[1],
                            ResultSelector.Body,
                            clauseGenerationContext),
                        ResultSelector.Parameters[0],
                        clauseGenerationContext));
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override QueryModel ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            var nestClause = new NestClause(
                ResultSelector.Parameters[1].Name, 
                ResultSelector.Parameters[1].Type, 
                InnerSequence, 
                GetResolvedKeySelector(clauseGenerationContext),
                IsLeftOuterNest);

            clauseGenerationContext.AddContextInfo(this, nestClause);
            queryModel.BodyClauses.Add(nestClause);

            var selectClause = queryModel.SelectClause;
            selectClause.Selector = GetResolvedResultSelector(clauseGenerationContext);

            return queryModel;
        }
    }
}