using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class DictionaryContainsKeyMethodCallTranslator  : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (IDictionary).GetMethod("Contains")!,
            typeof (IDictionary<,>).GetMethod("ContainsKey")!
        };

        public IEnumerable<MethodInfo> SupportMethods => SupportedMethodsStatic;

        public Expression Translate(MethodCallExpression methodCallExpression, N1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException(nameof(methodCallExpression));
            }

            if (methodCallExpression.Arguments[0] is ConstantExpression keyExpression)
            {
                var expression = expressionTreeVisitor.Expression;

                expressionTreeVisitor.Visit(methodCallExpression.Object!);
                expression.Append('.');
                expression.Append(N1QlHelpers.EscapeIdentifier(keyExpression.Value!.ToString()!));
                expression.Append(" IS NOT MISSING");
            }
            else
            {
                throw new NotSupportedException("Dictionary keys must be constants");
            }

            return methodCallExpression;
        }
    }
}
