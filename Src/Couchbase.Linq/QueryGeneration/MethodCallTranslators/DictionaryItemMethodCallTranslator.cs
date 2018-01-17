using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class DictionaryItemMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (IDictionary).GetMethod("get_Item"),
            typeof (IDictionary<,>).GetMethod("get_Item")
        };

        public IEnumerable<MethodInfo> SupportMethods => SupportedMethodsStatic;

        public Expression Translate(MethodCallExpression methodCallExpression, N1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException("methodCallExpression");
            }

            if (methodCallExpression.Arguments[0] is ConstantExpression keyExpression)
            {
                var expression = expressionTreeVisitor.Expression;

                expressionTreeVisitor.Visit(methodCallExpression.Object);
                expression.Append('.');
                expression.Append(N1QlHelpers.EscapeIdentifier(keyExpression.Value.ToString()));
            }
            else
            {
                throw new NotSupportedException("Dictionary keys must be constants");
            }

            return methodCallExpression;
        }
    }
}
