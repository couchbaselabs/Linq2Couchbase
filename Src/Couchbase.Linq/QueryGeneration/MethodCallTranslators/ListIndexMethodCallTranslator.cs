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
    internal class ListIndexMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (IList).GetMethod("get_Item"),
            typeof (IList<>).GetMethod("get_Item")
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

            expressionTreeVisitor.VisitExpression(methodCallExpression.Object);
            expression.Append('[');
            expressionTreeVisitor.VisitExpression(methodCallExpression.Arguments[0]);
            expression.Append(']');

            return methodCallExpression;
        }
    }
}
