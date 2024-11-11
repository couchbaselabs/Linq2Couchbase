using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class SubqueryMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (Enumerable).GetMethod("ToArray")!,
            typeof (Enumerable).GetMethod("ToList")!,
            typeof (Enumerable).GetMethod("AsEnumerable")!
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

            // Do not process ToArray, ToList, or AsEnumerable.  Instead just visit the list in the first argument.

            expressionTreeVisitor.Visit(methodCallExpression.Arguments[0]);

            return methodCallExpression;
        }
    }
}
