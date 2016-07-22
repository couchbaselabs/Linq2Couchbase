using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    internal class ExtentNameExpressionNode : MethodCallExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
            { typeof(EnumerableExtensions).GetMethod("AsQueryable") };

        /// <summary>
        /// The <see cref="QueryModel.MainFromClause"/> will have an <see cref="IQuerySource.ItemName"/> composed of
        /// this prefix followed by the <see cref="ExtentName"/>.
        /// </summary>
        public const string ItemNamePrefix = "__ExtentName_";

        public ExtentNameExpressionNode(MethodCallExpressionParseInfo parseInfo, ConstantExpression extentName)
            : base(parseInfo)
        {
            if (extentName == null)
            {
                throw new ArgumentNullException("extentName");
            }
            if (extentName.Type != typeof(string))
            {
                throw new ArgumentException("extentName must return a string", "extentName");
            }

            ExtentName = extentName;
        }

        public ConstantExpression ExtentName { get; private set; }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            queryModel.MainFromClause.ItemName = ItemNamePrefix + (string) ExtentName.Value;
        }
    }
}