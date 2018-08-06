using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Visits an expression tree, applying <see cref="ISerializationConverter{T}"/> calls when reading or writing
    /// from members with custom serialization.
    /// </summary>
    internal class SerializationExpressionTreeVisitor : RelinqExpressionVisitor
    {
        private static readonly HashSet<ExpressionType> ComparisonExpressionTypes = new HashSet<ExpressionType>
        {
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.Equal,
            ExpressionType.NotEqual
        };

        private readonly ISerializationConverterProvider _converterProvider;

        public SerializationExpressionTreeVisitor(ISerializationConverterProvider converterProvider)
        {
            _converterProvider = converterProvider ?? throw new ArgumentNullException(nameof(converterProvider));
        }

        /// <inheritdoc/>
        /// <summary>
        /// When getting a property with a serialization converter applied, convert it from the specialized
        /// storage format to the more standardized format
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            node = node.Update(Visit(node.Expression));

            var converter = _converterProvider.GetSerializationConverter(node.Member);
            if (converter != null)
            {
                return converter.GenerateConvertFromExpression(node);
            }

            return node;
        }

        /// <inheritdoc/>
        /// <summary>
        /// When assigning values to a property with a custom serializer, such as in a select projection,
        /// convert them to the custom format in N1QL so they can be correctly deserialized.
        /// </summary>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var converter = _converterProvider.GetSerializationConverter(node.Member);
            if (converter != null)
            {
                return node.Update(
                    converter.GenerateConvertToExpression(
                        Visit(node.Expression)));
            }

            return base.VisitMemberAssignment(node);
        }

        /// <inheritdoc/>
        /// <summary>
        /// For comparisons, we want to default to comparing in the format already stored in JSON.
        /// So if one side is a call to ConvertFrom, remove it and place ConvertTo on the other side instead.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (!ComparisonExpressionTypes.Contains(node.NodeType))
            {
                return base.VisitBinary(node);
            }

            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var leftConvertMethod = ExtractConvertFromMethod(left);
            if (leftConvertMethod != null)
            {
                var inverseMethod = leftConvertMethod.Method.DeclaringType?.GetMethod("ConvertTo");
                if (inverseMethod != null)
                {
                    return node.Update(
                        leftConvertMethod.Arguments[0],
                        node.Conversion,
                        Expression.Call(
                            leftConvertMethod.Object,
                            inverseMethod,
                            right));
                }
            }

            var rightConvertMethod = ExtractConvertFromMethod(right);
            if (rightConvertMethod != null)
            {
                var inverseMethod = rightConvertMethod.Method.DeclaringType?.GetMethod("ConvertTo");
                if (inverseMethod != null)
                {
                    return node.Update(
                        Expression.Call(
                            rightConvertMethod.Object,
                            inverseMethod,
                            left)
                        ,
                        node.Conversion,
                        rightConvertMethod.Arguments[0]);
                }
            }

            // Fallback behavior
            return node.Update(
                left,
                node.Conversion,
                right);
        }

        private static MethodCallExpression ExtractConvertFromMethod(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression &&
                     methodCallExpression.Method.DeclaringType != null)
            {
                if (methodCallExpression.Method.DeclaringType.GetTypeInfo().IsInterface &&
                    methodCallExpression.Method.DeclaringType.GetTypeInfo().IsGenericType &&
                    methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(ISerializationConverter<>))
                {
                    if (methodCallExpression.Method.Name == "ConvertFrom")
                    {
                        return methodCallExpression;
                    }
                }
            }

            return null;
        }
    }
}
