using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    class StringTrimMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic =
        {
            typeof (string).GetMethod("Trim", Type.EmptyTypes),
            typeof (string).GetMethod("Trim", new[] { typeof (char[]) }),
            typeof (string).GetMethod("TrimStart"),
            typeof (string).GetMethod("TrimEnd")
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

            expression.Append(methodCallExpression.Method.Name == "TrimStart" ? "LTRIM(" :
                methodCallExpression.Method.Name == "TrimEnd" ? "RTRIM(" : "TRIM(");
            expressionTreeVisitor.VisitExpression(methodCallExpression.Object);

            if (methodCallExpression.Arguments.Count > 0)
            {
                if (methodCallExpression.Arguments[0].Type != typeof (char[]))
                {
                    throw new NotSupportedException("String Trim Operations Expect Character Array Parameters");
                }

                try
                {
                    var lambda = Expression.Lambda<Func<char[]>>(methodCallExpression.Arguments[0]).Compile();
                    var chars = lambda.Invoke();

                    if ((chars != null) && (chars.Length > 0))
                    {
                        expression.Append(", ");

                        expressionTreeVisitor.VisitExpression(Expression.Constant(new String(chars), typeof (string)));
                    }
                }
                catch (NotSupportedException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException("Unable To Parse Trim Character Set.  Dynamic Expressions Are Not Supported", ex);
                }
            }

            expression.Append(")");

            return methodCallExpression;
        }
    }
}
