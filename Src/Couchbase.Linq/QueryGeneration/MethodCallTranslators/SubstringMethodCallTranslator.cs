using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class SubstringMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (string).GetMethod("Substring", new[] { typeof (int) }),
            typeof (string).GetMethod("Substring", new[] { typeof (int), typeof(int) }),
            typeof (string).GetMethod("get_Chars", new[] { typeof (int) })
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

            var expression = expressionTreeVisitor.Expression;

            expression.Append("SUBSTR(");
            expressionTreeVisitor.VisitExpression(methodCallExpression.Object);
            expression.Append(", ");
            expressionTreeVisitor.VisitExpression(methodCallExpression.Arguments[0]);

            if (methodCallExpression.Arguments.Count > 1)
            {
                expression.Append(", ");
                expressionTreeVisitor.VisitExpression(methodCallExpression.Arguments[1]);
            }
            else if (methodCallExpression.Method.Name == "get_Chars")
            {
                // Called str[i], so return a single character at i
                expression.Append(", 1");
            }

            expression.Append(")");

            return methodCallExpression;
        }
    }
}
