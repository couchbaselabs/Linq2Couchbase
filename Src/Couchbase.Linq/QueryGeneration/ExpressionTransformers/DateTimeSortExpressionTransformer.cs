using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Serialization;
using Couchbase.Linq.Serialization.Converters;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Converts any DateTime properties being used in OrderBy clauses to Unix milliseconds before sorting them.
    /// This way the <see cref="MethodCallTranslators.SerializationConverterMethodCallTranslator">SerializationConverterMethodCallTranslator</see>
    /// will later interpret the calls as STR_TO_MILLIS() calls in N1QL.  N1QL can't accurately sort ISO8601
    /// date/time values unless they're converted to Unix milliseconds first. It may also be important for efficient index usage.
    /// </summary>
    internal class DateTimeSortExpressionTransformer : IExpressionTransformer<MethodCallExpression>
    {
        private static readonly UnixMillisecondsSerializationConverter Converter = new UnixMillisecondsSerializationConverter();

        private static readonly HashSet<Type> Types = new HashSet<Type>
        {
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?)
        };

        private static readonly HashSet<MethodInfo> Methods = new HashSet<MethodInfo>
        {
            typeof(Queryable).GetMethods().Single(p => p.Name == nameof(Queryable.OrderBy) && p.GetParameters().Length == 2),
            typeof(Queryable).GetMethods().Single(p => p.Name == nameof(Queryable.OrderByDescending) && p.GetParameters().Length == 2),
            typeof(Queryable).GetMethods().Single(p => p.Name == nameof(Queryable.ThenBy) && p.GetParameters().Length == 2),
            typeof(Queryable).GetMethods().Single(p => p.Name == nameof(Queryable.ThenByDescending) && p.GetParameters().Length == 2)
        };

        private static readonly HashSet<MethodInfo> InverseConversionMethods = new HashSet<MethodInfo>(
            Types.Select(type => typeof(ISerializationConverter<>).MakeGenericType(type)
                .GetMethod(nameof(ISerializationConverter<DateTime>.ConvertFrom), new[] {type}))!);

        private static readonly ExpressionType[] StaticSupportedExpressionTypes =
        {
            ExpressionType.Call
        };

        public ExpressionType[] SupportedExpressionTypes => StaticSupportedExpressionTypes;

        public Expression Transform(MethodCallExpression expression)
        {
            var method = expression.Method;
            if (!method.IsGenericMethod || !Methods.Contains(method.GetGenericMethodDefinition()))
            {
                // This isn't a sort
                return expression;
            }

            var sortArgument = expression.Arguments[1];
            var newSortArgument = TransformLambda(sortArgument);

            if (sortArgument == newSortArgument)
            {
                // No change
                return expression;
            }

            return expression.Update(
                expression.Object,
                expression.Arguments.Select((p, i) => i == 1 ? newSortArgument : p));
        }

        private static Expression TransformExpression(Expression expression) =>
            Converter.GenerateConvertToExpression(expression);

        private static Expression TransformLambda(Expression expression)
        {
            UnaryExpression? unaryExpression = expression as UnaryExpression;

            var lambdaExpression = unaryExpression != null
                ? unaryExpression.Operand as LambdaExpression
                : expression as LambdaExpression;

            if (lambdaExpression == null)
            {
                return expression;
            }

            var sortExpression = lambdaExpression.Body;
            if (!Types.Contains(sortExpression.Type))
            {
                // We're not sorting by a date/time property, so ignore
                return expression;
            };

            if (sortExpression.NodeType != ExpressionType.MemberAccess)
            {
                // This isn't a simple property access sort. The only time we wrap that is if we're wrapping
                // an existing inverse conversion. In that case, we'll wrap so that it's converted and then
                // converted back. This gets translated into a no-op during query generation. We can't drop
                // the conversions here because otherwise it gets wrapped again on the subsequent visit.

                if (!(sortExpression is MethodCallExpression methodCallExpression) ||
                    !InverseConversionMethods.Contains(methodCallExpression.Method))
                {
                    return expression;
                }
            }

            var newBody = TransformExpression(sortExpression);
            var newLambda = Expression.Lambda(newBody, lambdaExpression.Parameters);

            return unaryExpression?.Update(newLambda) ?? (Expression) newLambda;
        }
    }
}
