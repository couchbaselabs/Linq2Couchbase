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
        private const string Contains = "Contains";
        private const string StartsWith = "StartsWith";
        private const string EndsWith = "EndsWith";

        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof(string).GetMethod(Contains, new[] {typeof(string)}),
            typeof(string).GetMethod(StartsWith, new[] {typeof(string)}),
            typeof(string).GetMethod(EndsWith, new[] {typeof(string)})
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

            expression.Append("(");
            expressionTreeVisitor.Visit(methodCallExpression.Object);
            expression.Append(" LIKE ");

            var constantExpression = methodCallExpression.Arguments[0] as ConstantExpression;
            if ((constantExpression != null) && (constantExpression.Type == typeof(string)) &&
                (constantExpression.Value != null))
            {
                // This is a string constant, so we can build the literal in advance

                var newExpression = Expression.Constant(
                    (methodCallExpression.Method.Name != StartsWith ? "%" : "") +
                    constantExpression.Value +
                    (methodCallExpression.Method.Name != EndsWith ? "%" : ""));

                expressionTreeVisitor.Visit(newExpression);
            }
            else
            {
                // The argument is dynamic, so we must build the comparison in N1QL

                if (methodCallExpression.Method.Name != StartsWith)
                {
                    expression.Append("'%' || ");
                }

                expressionTreeVisitor.Visit(methodCallExpression.Arguments[0]);

                if (methodCallExpression.Method.Name != EndsWith)
                {
                    expression.Append(" || '%'");
                }
            }

            expression.Append(')');

            return methodCallExpression;
        }
    }
}
