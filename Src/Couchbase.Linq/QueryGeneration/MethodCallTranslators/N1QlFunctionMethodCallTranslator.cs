using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    /// <summary>
    /// Translates calls to static methods decorated with <see cref="N1QlFunctionAttribute" />.
    /// </summary>
    internal class N1QlFunctionMethodCallTranslator : IMethodCallTranslator
    {
        public IEnumerable<MethodInfo> SupportMethods
        {
            get { return new[] { MethodInfo }; }
        }

        public MethodInfo MethodInfo { get; private set; }
        public string N1QlFunctionName { get; private set; }

        /// <summary>
        /// Creates a new N1QlFunctionMethodCallTranslator for a given method and <see cref="N1QlFunctionAttribute"/>.
        /// </summary>
        /// <param name="methodInfo">Method call to be translated.  Must be a static method.</param>
        /// <param name="attribute"><see cref="N1QlFunctionAttribute"/> that defines the translation.</param>
        public N1QlFunctionMethodCallTranslator(MethodInfo methodInfo, N1QlFunctionAttribute attribute)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }
            if (!methodInfo.IsStatic)
            {
                throw new ArgumentException("Only static methods are supported", "methodInfo");
            }
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            MethodInfo = methodInfo;
            N1QlFunctionName = attribute.N1QlFunctionName;
        }

        /// <summary>
        /// Translate the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression">Method call to be translated.  Must match the method provided to the constructor.</param>
        /// <param name="expressionTreeVisitor"><see cref="N1QlExpressionTreeVisitor"/> to use to visit parameters.</param>
        /// <returns>Original or altered expression.</returns>
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
            if (methodCallExpression.Method != MethodInfo)
            {
                throw new ArgumentException("Cannot translate a method other than the one provided to the constructor.", "methodCallExpression");
            }

            expressionTreeVisitor.Expression.Append(N1QlFunctionName);
            expressionTreeVisitor.Expression.Append('(');

            for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
            {
                if (i > 0)
                {
                    expressionTreeVisitor.Expression.Append(", ");
                }

                expressionTreeVisitor.Visit(methodCallExpression.Arguments[i]);
            }

            expressionTreeVisitor.Expression.Append(')');

            return methodCallExpression;
        }
    }
}
