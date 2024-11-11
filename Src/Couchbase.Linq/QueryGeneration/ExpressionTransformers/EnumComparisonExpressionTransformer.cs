using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Utils;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Recognizes == and != comparisons between a enumeration property and an enumeration constant.
    /// By default, LINQ does this comparison using the base type of the enumeration, converting the
    /// enumeration property to the base type and comparing to the raw number.  This converts the
    /// comparison to be directly between two values of the enumeration type.  This allows
    /// <see cref="N1QlExpressionTreeVisitor"/> to handle different serialization approaches for the
    /// enumeration constant.
    /// </summary>
    internal class EnumComparisonExpressionTransformer : IExpressionTransformer<BinaryExpression>
    {
        public ExpressionType[] SupportedExpressionTypes
        {
            get
            {
                return new[]
                {
                    ExpressionType.Equal,
                    ExpressionType.NotEqual
                };
            }
        }

        public Expression Transform(BinaryExpression expression)
        {
            Expression? newExpression = null;

            ConstantExpression? constant;
            if (IsEnumConversion(expression.Left) &&
                (constant = ReflectionUtils.UnwrapNullableConversion<ConstantExpression>(expression.Right, out _)) != null)
            {
                newExpression = MakeEnumComparisonExpression(
                    ((UnaryExpression) expression.Left).Operand,
                    constant.Value,
                    expression.NodeType);
            }
            else if (IsEnumConversion(expression.Right) &&
                     (constant = ReflectionUtils.UnwrapNullableConversion<ConstantExpression>(expression.Left, out _)) != null)
            {
                newExpression = MakeEnumComparisonExpression(
                    ((UnaryExpression)expression.Right).Operand,
                    constant.Value,
                    expression.NodeType);
            }

            return newExpression ?? expression;
        }

        private bool IsEnumConversion(Expression expression)
        {
            if (expression.NodeType != ExpressionType.Convert)
            {
                return false;
            }

            var convertExpression = (UnaryExpression) expression;
            var type = convertExpression.Operand.Type;

            // If type is Nullable<T>, extract the inner type to see if it is an enumeration
            if (type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }

            return type.GetTypeInfo().IsEnum;
        }

        private BinaryExpression? MakeEnumComparisonExpression(Expression operand, object? enumValue,
            ExpressionType comparisonType)
        {
            var isNullable = false;
            var enumType = operand.Type;

            // If type is Nullable<T>, the inner type is the enumeration type
            if (enumType.GetTypeInfo().IsGenericType && (enumType.GetGenericTypeDefinition() == typeof (Nullable<>)))
            {
                enumType = enumType.GetGenericArguments()[0];
                isNullable = true;
            }

            string? name = null;
            if (enumValue != null)
            {
                // enumValue == null if this is a Nullable type and it doesn't have a value

                name = Enum.GetName(enumType, enumValue);

                if (name == null)
                {
                    // Don't bother converting for undefined enumeration values, we'll use the original expression instead

                    return null;
                }
            }

            Expression comparisonValue;

            if (name != null)
            {
                comparisonValue = Expression.Constant(enumType.GetField(name)!.GetValue(null));

                if (isNullable)
                {
                    comparisonValue = Expression.Convert(comparisonValue, typeof(Nullable<>).MakeGenericType(enumType));
                }
            }
            else
            {
                comparisonValue = Expression.Constant(null);
            }

            return Expression.MakeBinary(comparisonType, operand, comparisonValue);
        }
    }
}
