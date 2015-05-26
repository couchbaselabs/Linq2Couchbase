using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// An expresssion parser for the "META" function.
    /// </summary>
    public class MetaExpressionNode: ResultOperatorExpressionNodeBase
    {
        public static MethodInfo[] SupportedMethods =
        {
            typeof (QueryExtensions).GetMethod("Meta")
        };

        public MetaExpressionNode(MethodCallExpressionParseInfo parseInfo,
            LambdaExpression optionalPredicate,
            LambdaExpression optionalSelector)
            : base(parseInfo, optionalPredicate, optionalSelector)
        {
        }

        protected override ResultOperatorBase CreateResultOperator(
            ClauseGenerationContext clauseGenerationContext)
        {
            return new MetaResultOperator();
        }

        public override Expression Resolve(ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);
        }
    }
}
