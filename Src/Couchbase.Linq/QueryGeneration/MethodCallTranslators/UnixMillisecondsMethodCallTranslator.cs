using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class UnixMillisecondsMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (UnixMillisecondsDateTime).GetMethod("FromDateTime", new[] { typeof (DateTime) }),
            typeof (UnixMillisecondsDateTime).GetMethod("FromDateTime", new[] { typeof (DateTime?) }),
            typeof (UnixMillisecondsDateTime).GetMethod("ToDateTime", new[] { typeof (UnixMillisecondsDateTime) }),
            typeof (UnixMillisecondsDateTime).GetMethod("ToDateTime", new[] { typeof (UnixMillisecondsDateTime?) })
        };

        public IEnumerable<MethodInfo> SupportMethods
        {
            get
            {
                return SupportedMethodsStatic;
            }
        }

        public Expression Translate(MethodCallExpression methodCallExpression, N1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException("methodCallExpression");
            }

            var argument = methodCallExpression.Arguments[0];
            var methodCallArgument = argument as MethodCallExpression;

            if ((methodCallArgument != null) && SupportMethods.Contains(methodCallArgument.Method))
            {
                // Two method calls are reversing each other, so just skip them both

                return expressionTreeVisitor.Visit(methodCallArgument.Arguments[0]);
            }

            var expression = expressionTreeVisitor.Expression;

            if (methodCallExpression.Method.Name == "FromDateTime")
            {
                expression.Append("STR_TO_MILLIS(");
            }
            else
            {
                expression.Append("MILLIS_TO_STR(");
            }

            expressionTreeVisitor.Visit(argument);

            expression.Append(')');

            return methodCallExpression;
        }
    }
}
