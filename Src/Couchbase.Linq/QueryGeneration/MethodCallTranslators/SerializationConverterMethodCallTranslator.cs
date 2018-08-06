using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Serialization;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class SerializationConverterMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (ISerializationConverter<>).GetMethod("ConvertTo"),
            typeof (ISerializationConverter<>).GetMethod("ConvertFrom")
        };

        public IEnumerable<MethodInfo> SupportMethods => SupportedMethodsStatic;

        public Expression Translate(MethodCallExpression methodCallExpression, N1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException(nameof(methodCallExpression));
            }
            if (expressionTreeVisitor == null)
            {
                throw new ArgumentNullException(nameof(expressionTreeVisitor));
            }

            if (methodCallExpression.Object is ConstantExpression constantExpression
                && constantExpression.Value is ISerializationConverter conversion)
            {
                if (methodCallExpression.Method.Name == "ConvertTo")
                {
                    conversion.RenderConvertTo(methodCallExpression.Arguments[0], expressionTreeVisitor);
                }
                else
                {
                    conversion.RenderConvertFrom(methodCallExpression.Arguments[0], expressionTreeVisitor);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid attempt to process ISerializationConverter<T> method call.");
            }

            return methodCallExpression;
        }
    }
}
