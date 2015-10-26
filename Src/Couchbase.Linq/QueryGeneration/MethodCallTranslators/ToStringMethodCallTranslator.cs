using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class ToStringMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (object).GetMethod("ToString")
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
            if (methodCallExpression.Object == null)
            {
                throw new InvalidOperationException();
            }

            if (methodCallExpression.Object.Type == typeof (string))
            {
                expressionTreeVisitor.VisitExpression(methodCallExpression.Object);
            }
            else
            {
                var expression = expressionTreeVisitor.Expression;

                expression.Append("TOSTRING(");
                expressionTreeVisitor.VisitExpression(methodCallExpression.Object);
                expression.Append(")");
            }

            return methodCallExpression;
        }
    }
}
