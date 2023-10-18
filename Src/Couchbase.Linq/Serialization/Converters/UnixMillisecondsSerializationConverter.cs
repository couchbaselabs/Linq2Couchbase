using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Core.Utils;
using Couchbase.Linq.QueryGeneration;

namespace Couchbase.Linq.Serialization.Converters
{
    /// <summary>
    /// Implementation of <see cref="ISerializationConverter{T}"/> for handling date/time properties
    /// flagged with the <see cref="Couchbase.Core.IO.Serializers.UnixMillisecondsConverter"/>.
    /// </summary>
    public class UnixMillisecondsSerializationConverter : SerializationConverterBase,
        ISerializationConverter<DateTime>, ISerializationConverter<DateTime?>,
        ISerializationConverter<DateTimeOffset>, ISerializationConverter<DateTimeOffset?>
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly IDictionary<Type, MethodInfo> ConvertFromMethodsStatic =
            GetConvertFromMethods<UnixMillisecondsSerializationConverter>();

        private static readonly IDictionary<Type, MethodInfo> ConvertToMethodsStatic =
            GetConvertToMethods<UnixMillisecondsSerializationConverter>();

        /// <inheritdoc/>
        protected override IDictionary<Type, MethodInfo> ConvertFromMethods => ConvertFromMethodsStatic;

        /// <inheritdoc/>
        protected override IDictionary<Type, MethodInfo> ConvertToMethods => ConvertToMethodsStatic;

        /// <inheritdoc/>
        DateTime ISerializationConverter<DateTime>.ConvertTo(DateTime value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTime? ISerializationConverter<DateTime?>.ConvertTo(DateTime? value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTimeOffset ISerializationConverter<DateTimeOffset>.ConvertTo(DateTimeOffset value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTimeOffset? ISerializationConverter<DateTimeOffset?>.ConvertTo(DateTimeOffset? value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTime ISerializationConverter<DateTime>.ConvertFrom(DateTime value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTime? ISerializationConverter<DateTime?>.ConvertFrom(DateTime? value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTimeOffset ISerializationConverter<DateTimeOffset>.ConvertFrom(DateTimeOffset value)
        {
            return value;
        }

        /// <inheritdoc/>
        DateTimeOffset? ISerializationConverter<DateTimeOffset?>.ConvertFrom(DateTimeOffset? value)
        {
            return value;
        }

        /// <inheritdoc/>
        protected override void RenderConvertToMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            expressionTreeVisitor.Expression.Append("STR_TO_MILLIS(");
            expressionTreeVisitor.Visit(innerExpression);
            expressionTreeVisitor.Expression.Append(')');
        }

        /// <inheritdoc/>
        protected override void RenderConvertFromMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            expressionTreeVisitor.Expression.Append("MILLIS_TO_STR(");
            expressionTreeVisitor.Visit(innerExpression);
            expressionTreeVisitor.Expression.Append(')');
        }

        /// <inheritdoc/>
        protected override void RenderConvertedConstant(ConstantExpression constantExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            if (constantExpression.Value == null)
            {
                // Don't try to convert nulls
                expressionTreeVisitor.Visit(constantExpression);
            }
            else
            {
                var dateTime = GetDateTime(constantExpression);
                var unixMilliseconds = (dateTime - UnixEpoch).TotalMilliseconds;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Math.Floor(unixMilliseconds) == unixMilliseconds)
                {
                    expressionTreeVisitor.Expression.Append((long) unixMilliseconds);
                }
                else
                {
#if NET6_0_OR_GREATER
                    Span<char> buffer = stackalloc char[32];
                    var chars =
                        unixMilliseconds.TryFormat(buffer, out var charsWritten, provider: CultureInfo.InvariantCulture)
                            ? buffer[..charsWritten]
                            : unixMilliseconds.ToStringInvariant().AsSpan();
#else
                    var chars = unixMilliseconds.ToStringInvariant();
#endif

                    expressionTreeVisitor.Expression.Append(chars);
                }
            }
        }

        private static DateTime GetDateTime(ConstantExpression constantExpression)
        {
            switch (constantExpression.Value)
            {
                case DateTime dateTime:
                    if (dateTime.Kind == DateTimeKind.Local)
                    {
                        dateTime = dateTime.ToUniversalTime();
                    }

                    return dateTime;

                case DateTimeOffset offset:
                    return offset.UtcDateTime;

                default:
                    throw new InvalidOperationException("ConstantExpression is not a DateTime or equivalent");
            }
        }
    }
}
