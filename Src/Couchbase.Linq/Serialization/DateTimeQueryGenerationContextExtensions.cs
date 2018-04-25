using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.QueryGeneration;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Extensions to <see cref="N1QlQueryGenerationContext"/> to assist with DateTime serialization.
    /// </summary>
    internal static class DateTimeQueryGenerationContextExtensions
    {
        /// <summary>
        /// Tests an <see cref="Expression"/> returning a DateTime to see if it is serialized as
        /// Unix milliseconds using the <see cref="N1QlQueryGenerationContext.DateTimeSerializationFormatProvider"/>.
        /// </summary>
        /// <param name="context">The <see cref="N1QlQueryGenerationContext"/>.</param>
        /// <param name="expression">Expression which returns a DateTime.</param>
        /// <returns>True if the member is serialized as Unix milliseconds.</returns>
        public static bool IsUnixMillisecondsMember(this N1QlQueryGenerationContext context, Expression expression)
        {
            var memberInfo = ExtractMemberInfo(expression);

            if (memberInfo != null)
            {
                return context.DateTimeSerializationFormatProvider.GetDateTimeSerializationFormat(memberInfo) ==
                       DateTimeSerializationFormat.UnixMilliseconds;
            }

            return false;
        }

        private static MemberInfo ExtractMemberInfo(Expression expression)
        {
            if (expression is UnaryExpression convertExpression)
            {
                if (convertExpression.NodeType == ExpressionType.Convert && convertExpression.IsLifted)
                {
                    var typeInfo = convertExpression.Operand.Type.GetTypeInfo();

                    if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        // Expression is a call to Nullable<DateTime>.Value, so we should really be testing the inner operand

                        expression = convertExpression.Operand;
                    }
                }
            }

            return (expression as MemberExpression)?.Member;
        }
    }
}
