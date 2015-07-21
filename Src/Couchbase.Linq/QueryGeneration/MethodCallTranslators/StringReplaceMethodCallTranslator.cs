using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    class StringReplaceMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (string).GetMethod("Replace", new[] { typeof (char), typeof(char) }),
            typeof (string).GetMethod("Replace", new[] { typeof (string), typeof(string) })
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

            expression.Append("REPLACE(");
            expressionTreeVisitor.VisitExpression(methodCallExpression.Object);
            expression.Append(", ");
            expressionTreeVisitor.VisitExpression(methodCallExpression.Arguments[0]);
            expression.Append(", ");
            expressionTreeVisitor.VisitExpression(methodCallExpression.Arguments[1]);
            expression.Append(")");

            return methodCallExpression;
        }
    }
}
