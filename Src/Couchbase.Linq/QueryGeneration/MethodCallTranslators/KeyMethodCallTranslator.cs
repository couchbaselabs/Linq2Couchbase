using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class KeyMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (N1QlFunctions).GetMethod("Key")!
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
            if (expressionTreeVisitor == null)
            {
                throw new ArgumentNullException("expressionTreeVisitor");
            }

            var expression = expressionTreeVisitor.Expression;

            expression.Append("META(");
            expressionTreeVisitor.Visit(methodCallExpression.Arguments[0]);
            expression.Append(").id");

            return methodCallExpression;
        }
    }
}
