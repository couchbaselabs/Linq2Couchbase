using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Serialization;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class UnixMillisecondsMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (UnixMillisecondsDateTime).GetMethod("FromDateTime", new[] { typeof (DateTime) }),
            typeof (UnixMillisecondsDateTime).GetMethod("FromDateTime", new[] { typeof (DateTime?) }),
            typeof (UnixMillisecondsDateTimeOffset).GetMethod("FromDateTimeOffset", new[] { typeof (DateTimeOffset) }),
            typeof (UnixMillisecondsDateTimeOffset).GetMethod("FromDateTimeOffset", new[] { typeof (DateTimeOffset?) }),
            typeof (UnixMillisecondsDateTime).GetMethod("ToDateTime", new[] { typeof (UnixMillisecondsDateTime) }),
            typeof (UnixMillisecondsDateTime).GetMethod("ToDateTime", new[] { typeof (UnixMillisecondsDateTime?) }),
            typeof (UnixMillisecondsDateTimeOffset).GetMethod("ToDateTimeOffset", new[] { typeof (UnixMillisecondsDateTimeOffset) }),
            typeof (UnixMillisecondsDateTimeOffset).GetMethod("ToDateTimeOffset", new[] { typeof (UnixMillisecondsDateTimeOffset?) })
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

            var argument = methodCallExpression.Arguments[0];

            if (argument is MethodCallExpression methodCallArgument && SupportMethods.Contains(methodCallArgument.Method))
            {
                // Two method calls are reversing each other, so just skip them both

                return expressionTreeVisitor.Visit(methodCallArgument.Arguments[0]);
            }

            var expression = expressionTreeVisitor.Expression;

            var needClosingParens = false;
            if (methodCallExpression.Method.Name == "FromDateTime" || methodCallExpression.Method.Name == "FromDateTimeOffset")
            {
                if (!expressionTreeVisitor.QueryGenerationContext.IsUnixMillisecondsMember(argument))
                {
                    expression.Append("STR_TO_MILLIS(");
                    needClosingParens = true;
                }
            }
            else
            {
                expression.Append("MILLIS_TO_STR(");
                needClosingParens = true;
            }

            expressionTreeVisitor.Visit(argument);

            if (needClosingParens)
            {
                expression.Append(')');
            }

            return methodCallExpression;
        }
    }
}
