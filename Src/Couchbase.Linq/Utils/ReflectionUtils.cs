using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Couchbase.Linq.Utils
{
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Unwraps a conversion to <see cref="Nullable{T}"/>, but only if the
        /// operand is of the given type <typeparamref name="T"/>.  If the expression
        /// is already of type <typeparamref name="T"/> it is returned without modification.
        /// </summary>
        /// <typeparam name="T">Operand type restriction.  Use <see cref="Expression"/> to unwrap any operand type.</typeparam>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="wasUnwrapped">True if the expression was unwrapped.</param>
        /// <returns>
        /// The unwrapped expression if the expression was a converstion of an operand of type <typeparamref name="T"/>.
        /// Otherwise, returns the original expression or null if the original expression is not of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: MaybeNull]
        public static T UnwrapNullableConversion<T>(Expression expression, out bool wasUnwrapped)
            where T: Expression
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression is UnaryExpression unaryExpression &&
                unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Type.GetTypeInfo().IsGenericType &&
                    unaryExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    wasUnwrapped = true;
                    return unaryExpression.Operand as T;
                }
            }

            wasUnwrapped = false;
            return expression as T;
        }

        public static Type? UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type);
    }
}
