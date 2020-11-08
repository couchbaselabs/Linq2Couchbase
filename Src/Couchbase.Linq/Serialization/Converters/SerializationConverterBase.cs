using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.QueryGeneration;

namespace Couchbase.Linq.Serialization.Converters
{
    public abstract class SerializationConverterBase : ISerializationConverter
    {
        /// <summary>
        /// Dictionary of <see cref="ISerializationConverter{T}.ConvertFrom"/> methods implemented
        /// by this type, indexed by the type being converted.
        /// </summary>
        /// <remarks>
        /// Implementation should return a statically allocated dictionary for performance.
        /// </remarks>
        protected abstract IDictionary<Type, MethodInfo> ConvertFromMethods { get; }

        /// <summary>
        /// Dictionary of <see cref="ISerializationConverter{T}.ConvertTo"/> methods implemented
        /// by this type, indexed by the type being converted.
        /// </summary>
        /// <remarks>
        /// Implementation should return a statically allocated dictionary for performance.
        /// </remarks>
        protected abstract IDictionary<Type, MethodInfo> ConvertToMethods { get; }

        /// <summary>
        /// Renders a conversion from the standard format to the custom format onto a N1QL query.
        /// </summary>
        /// <param name="innerExpression">The expression being converted.</param>
        /// <param name="expressionTreeVisitor"><see cref="IN1QlExpressionTreeVisitor"/> rendering the query.</param>
        protected abstract void RenderConvertToMethod(Expression innerExpression,
            IN1QlExpressionTreeVisitor expressionTreeVisitor);

        /// <summary>
        /// Renders a conversion from the custom format to the standard format onto a N1QL query.
        /// </summary>
        /// <param name="innerExpression">The expression being converted.</param>
        /// <param name="expressionTreeVisitor"><see cref="IN1QlExpressionTreeVisitor"/> rendering the query.</param>
        protected abstract void RenderConvertFromMethod(Expression innerExpression,
            IN1QlExpressionTreeVisitor expressionTreeVisitor);

        /// <summary>
        /// Renders a constant which has been preconverted to the custom format onto a N1QL query.
        /// </summary>
        /// <param name="constantExpression">The expression being converted.</param>
        /// <param name="expressionTreeVisitor"><see cref="IN1QlExpressionTreeVisitor"/> rendering the query.</param>
        protected abstract void RenderConvertedConstant(ConstantExpression constantExpression,
            IN1QlExpressionTreeVisitor expressionTreeVisitor);

        /// <inheritdoc/>
        public Expression GenerateConvertToExpression(Expression innerExpression)
        {
            if (ConvertToMethods.TryGetValue(innerExpression.Type, out var conversionMethod))
            {
                return ApplyConversion(innerExpression, conversionMethod);
            }

            return innerExpression;
        }

        /// <inheritdoc/>
        public Expression GenerateConvertFromExpression(Expression innerExpression)
        {
            if (ConvertFromMethods.TryGetValue(innerExpression.Type, out var conversionMethod))
            {
                return ApplyConversion(innerExpression, conversionMethod);
            }

            return innerExpression;
        }

        /// <inheritdoc/>
        public virtual void RenderConvertTo(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            var unwrapped = UnwrapInverseMethod(innerExpression, ConvertFromMethods.Values);
            if (unwrapped != null)
            {
                expressionTreeVisitor.Visit(unwrapped);
            }
            else if (innerExpression is ConstantExpression constantExpression)
            {
                RenderConvertedConstant(constantExpression, expressionTreeVisitor);
            }
            else
            {
                RenderConvertToMethod(innerExpression, expressionTreeVisitor);
            }
        }

        /// <inheritdoc/>
        public virtual void RenderConvertFrom(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            var unwrapped = UnwrapInverseMethod(innerExpression, ConvertToMethods.Values);
            if (unwrapped != null)
            {
                expressionTreeVisitor.Visit(unwrapped);
            }
            else
            {
                RenderConvertFromMethod(innerExpression, expressionTreeVisitor);
            }
        }

        /// <summary>
        /// Wraps an expression in a call to a conversion method.
        /// </summary>
        /// <param name="expression">Expression to wrap.</param>
        /// <param name="conversionMethod">The conversion method.</param>
        /// <returns>The wrapped expression.</returns>
        /// <remarks>
        /// The method must be an instance method of the class, and must accept a single parameter with the same type as the expression.
        /// </remarks>
        protected virtual Expression ApplyConversion(Expression expression, MethodInfo conversionMethod)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (conversionMethod == null)
            {
                throw new ArgumentNullException(nameof(conversionMethod));
            }

            return Expression.Call(
                Expression.Constant(this),
                conversionMethod,
                expression);
        }

        /// <summary>
        /// Tests to see if an expression is a method call to an inverse method, and if so it removes the call
        /// to the inverse method.
        /// </summary>
        /// <param name="expression">Expression to unwrap.</param>
        /// <param name="inverseMethods">List of inverse methods which should be removed.</param>
        /// <returns>The expression with the inverse method removed, or null if no inverse method was found.</returns>
        /// <remarks>
        /// This is a helper method which assists with removing unnecessary conversions.  If a conversion method is
        /// immediately followed by a call to its inverse, then neither is required.  Implementations of
        /// <see cref="RenderConvertTo(Expression, IN1QlExpressionTreeVisitor)"/> and <see cref="RenderConvertFrom(Expression, IN1QlExpressionTreeVisitor)"/>
        /// should use this method to avoid rendering unnecessary conversions.
        /// </remarks>
        protected virtual Expression? UnwrapInverseMethod(Expression expression, ICollection<MethodInfo> inverseMethods)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (inverseMethods == null)
            {
                throw new ArgumentNullException(nameof(inverseMethods));
            }

            if (inverseMethods.Count > 0)
            {
                // We must handle convert calls, these are created when dealing with Nullable<T>
                if (expression is UnaryExpression convertExpression &&
                    convertExpression.NodeType == ExpressionType.Convert)
                {
                    if (convertExpression.Operand is MethodCallExpression convertMethodCall &&
                        inverseMethods.Contains(convertMethodCall.Method))
                    {
                        return convertExpression.Update(convertMethodCall.Arguments[0]);
                    }
                }
                else if (expression is MethodCallExpression innerMethodCall &&
                         inverseMethods.Contains(innerMethodCall.Method))
                {
                    return innerMethodCall.Arguments[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Collects <see cref="ISerializationConverter{T}.ConvertFrom"/> implementations from a given class.
        /// </summary>
        /// <typeparam name="T">Class from which conversion methods are being extracted.</typeparam>
        /// <returns>Dictionary indexed by the type being converted.</returns>
        protected static IDictionary<Type, MethodInfo> GetConvertFromMethods<T>()
            where T: class
        {
            return GetConvertMethods(typeof(T), "ConvertFrom");
        }

        /// <summary>
        /// Collects <see cref="ISerializationConverter{T}.ConvertTo"/> implementations from a given class.
        /// </summary>
        /// <typeparam name="T">Class from which conversion methods are being extracted.</typeparam>
        /// <returns>Dictionary indexed by the type being converted.</returns>
        protected static IDictionary<Type, MethodInfo> GetConvertToMethods<T>()
            where T: class
        {
            return GetConvertMethods(typeof(T), "ConvertTo");
        }

        private static IDictionary<Type, MethodInfo> GetConvertMethods(Type type, string methodName)
        {
            return type
                .GetInterfaces()
                .Where(p => p.GetTypeInfo().IsGenericType && p.GetGenericTypeDefinition() == typeof(ISerializationConverter<>))
                .ToDictionary(
                    p => p.GetGenericArguments()[0],
                    p => p.GetMethod(methodName));
        }
    }
}

