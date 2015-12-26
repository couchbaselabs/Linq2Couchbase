using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class StringSplitMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (string).GetMethod("Split", new[] { typeof (char[]) })
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

            expression.Append("SPLIT(");
            expressionTreeVisitor.Visit(methodCallExpression.Object);

            if (methodCallExpression.Arguments[0].Type != typeof(char[]))
            {
                throw new NotSupportedException("String Split Operations Expect Character Array Parameters");
            }

            try
            {
                var lambda = Expression.Lambda<Func<char[]>>(methodCallExpression.Arguments[0]).Compile();
                var chars = lambda.Invoke();

                if ((chars != null) && (chars.Length > 0))
                {
                    if (chars.Length > 1)
                    {
                        throw new NotSupportedException("Cannot Split With More Than One Character");
                    }

                    expression.Append(", ");

                    expressionTreeVisitor.Visit(Expression.Constant(chars[0], typeof(char)));
                }
            }
            catch (NotSupportedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException("Unable To Parse Split Character Set.  Dynamic Expressions Are Not Supported", ex);
            }

            expression.Append(")");

            return methodCallExpression;
        }
    }
}
