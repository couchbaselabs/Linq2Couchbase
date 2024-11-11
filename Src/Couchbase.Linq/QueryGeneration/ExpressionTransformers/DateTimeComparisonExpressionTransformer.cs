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
    /// Converts any DateTime constants or properties to Unix milliseconds before comparing them.
    /// This way the <see cref="MethodCallTranslators.SerializationConverterMethodCallTranslator">SerializationConverterMethodCallTranslator</see>
    /// will later interpret the calls as STR_TO_MILLIS() calls in N1QL.  N1QL can't accurately compare ISO8601
    /// date/time values unless they're converted to Unix milliseconds first.
    /// </summary>
    internal class DateTimeComparisonExpressionTransformer : IExpressionTransformer<BinaryExpression>
    {
        private static readonly UnixMillisecondsSerializationConverter Converter = new UnixMillisecondsSerializationConverter();

        private static readonly HashSet<Type> Types = new HashSet<Type>
        {
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?)
        };

        private static readonly HashSet<MethodInfo> ConversionMethods = new HashSet<MethodInfo>(
            Types.Select(type => typeof(ISerializationConverter<>).MakeGenericType(type).GetMethod("ConvertTo", new[] {type}))!);
        private static readonly HashSet<MethodInfo> InverseConversionMethods = new HashSet<MethodInfo>(
            Types.Select(type => typeof(ISerializationConverter<>).MakeGenericType(type).GetMethod("ConvertFrom", new[] {type}))!);

        private static readonly ExpressionType[] StaticSupportedExpressionTypes =
        {
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.Equal,
            ExpressionType.NotEqual
        };

        public ExpressionType[] SupportedExpressionTypes => StaticSupportedExpressionTypes;

        public Expression Transform(BinaryExpression expression)
        {
            if ((expression.Left is MethodCallExpression leftMethodCall && ConversionMethods.Contains(leftMethodCall.Method)) ||
                (expression.Right is MethodCallExpression rightMethodCall && ConversionMethods.Contains(rightMethodCall.Method)))
            {
                // One side is already converted, which means SerializationExpressionTreeVisitor already did the conversion
                // So we can skip it

                return expression;
            }

            if (expression.Right is ConstantExpression constantExpression && constantExpression.Value == null)
            {
                // Testing for null, so don't do transformation unless the inner expression is already converted
                // In that case, drop the conversion to simplify the output query.

                if (expression.Left is MethodCallExpression methodCallExpression &&
                    InverseConversionMethods.Contains(methodCallExpression.Method))
                {
                    return expression.Update(
                        methodCallExpression.Arguments[0],
                        expression.Conversion,
                        expression.Right);
                }

                return expression;
            }

            return expression.Update(
                TransformSide(expression.Left),
                expression.Conversion,
                TransformSide(expression.Right));
        }

        private Expression TransformSide(Expression side)
        {
            if (!Types.Contains(side.Type))
            {
                return side;
            }

            if (side is MethodCallExpression methodCallExpression &&
                ConversionMethods.Contains(methodCallExpression.Method))
            {
                // Avoid infinite recursion, don't apply changes if we did it previously
                return side;
            }

            return Converter.GenerateConvertToExpression(side);
        }
    }
}
