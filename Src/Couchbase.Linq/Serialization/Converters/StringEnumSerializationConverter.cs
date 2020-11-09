using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.QueryGeneration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.Linq.Serialization.Converters
{
    /// <summary>
    /// Implementation of <see cref="ISerializationConverter{T}"/> for handling date/time properties
    /// flagged with the <see cref="Newtonsoft.Json.Converters.StringEnumConverter"/>.
    /// </summary>
    /// <typeparam name="T">Type of the enumeration.</typeparam>
    public class StringEnumSerializationConverter<T> : SerializationConverterBase,
        ISerializationConverter<T>, ISerializationConverter<T?>
        where T: struct
    {
        // ReSharper disable StaticMemberInGenericType
        private static readonly IDictionary<Type, MethodInfo> ConvertFromMethodsStatic =
            GetConvertFromMethods<StringEnumSerializationConverter<T>>();

        private static readonly IDictionary<Type, MethodInfo> ConvertToMethodsStatic =
            GetConvertToMethods<StringEnumSerializationConverter<T>>();
        // ReSharper restore StaticMemberInGenericType

        /// <inheritdoc/>
        protected override IDictionary<Type, MethodInfo> ConvertFromMethods => ConvertFromMethodsStatic;

        /// <inheritdoc/>
        protected override IDictionary<Type, MethodInfo> ConvertToMethods => ConvertToMethodsStatic;

        private readonly JsonConverter _jsonConverter;
        private readonly MemberInfo _member;

        /// <summary>
        /// Creates a new StringEnumSerializationConverter.
        /// </summary>
        /// <param name="jsonConverter">The <see cref="JsonConverter{T}"/> applied to the member.</param>
        /// <param name="member">The member being converted.</param>
        public StringEnumSerializationConverter(JsonConverter jsonConverter, MemberInfo member)
        {
            _jsonConverter = jsonConverter ?? throw new ArgumentNullException(nameof(jsonConverter));
            _member = member ?? throw new ArgumentNullException(nameof(member));
        }

        /// <inheritdoc/>
        T ISerializationConverter<T>.ConvertTo(T value)
        {
            return value;
        }

        /// <inheritdoc/>
        T? ISerializationConverter<T?>.ConvertTo(T? value)
        {
            return value;
        }

        /// <inheritdoc/>
        T ISerializationConverter<T>.ConvertFrom(T value)
        {
            return value;
        }

        /// <inheritdoc/>
        T? ISerializationConverter<T?>.ConvertFrom(T? value)
        {
            return value;
        }

        /// <inheritdoc/>
        protected override void RenderConvertToMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            // Don't try to convert in N1QL, for enums we're only supporting constants as strings
            expressionTreeVisitor.Visit(innerExpression);
        }

        /// <inheritdoc/>
        protected override void RenderConvertFromMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            // Don't try to convert in N1QL, for enums we're only supporting constants as strings
            expressionTreeVisitor.Visit(innerExpression);
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
                using (var writer = new JTokenWriter())
                {
                    _jsonConverter.WriteJson(writer, constantExpression.Value,
                        JsonSerializer.CreateDefault());

                    expressionTreeVisitor.Visit(Expression.Constant(writer.Token.ToString()));
                }
            }
        }
    }
}
