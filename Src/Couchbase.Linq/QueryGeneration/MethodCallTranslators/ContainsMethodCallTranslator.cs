using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class ContainsMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (string).GetMethod("Contains")
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

            expression.Append("(");
            expressionTreeVisitor.Visit(methodCallExpression.Object);
            expression.Append(" LIKE '%");

            var indexInsertStarted = expression.Length;

            expressionTreeVisitor.Visit(methodCallExpression.Arguments[0]);

            var indexInsertEnded = expression.Length;

            expression.Append("%')");

            //Remove extra quote marks which have been added due to the string in the clause, these aren't needed as they have been added already in this case.
            expression.Remove(indexInsertStarted, 1);
            expression.Remove(indexInsertEnded - 2, 1);

            return methodCallExpression;
        }
    }
}
