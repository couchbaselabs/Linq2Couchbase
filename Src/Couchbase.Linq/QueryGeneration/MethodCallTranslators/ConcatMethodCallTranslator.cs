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
    internal class ConcatMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (string).GetMethod("Concat", new[] { typeof (object) }),
            typeof (string).GetMethod("Concat", new[] { typeof (object), typeof (object) }),
            typeof (string).GetMethod("Concat", new[] { typeof (object), typeof (object), typeof (object) }),
            typeof (string).GetMethod("Concat", new[] { typeof (string), typeof (string) }),
            typeof (string).GetMethod("Concat", new[] { typeof (string), typeof (string), typeof (string) }),
            typeof (string).GetMethod("Concat", new[] { typeof (string), typeof (string), typeof (string), typeof (string) }),
            typeof (string).GetMethod("Concat", new[] { typeof (object[]) }),
            typeof (string).GetMethod("Concat", new[] { typeof (string[]) }),
            typeof (string).GetMethod("Concat", new[] { typeof (IEnumerable<string>) }),
            typeof (string).GetMethods().Single (mi => mi.Name == "Concat" && mi.IsGenericMethod && mi.GetGenericArguments().Length == 1)
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

            expression.Append('(');

            bool first = true;
            foreach (var argument in GetConcatenatedItems(methodCallExpression))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    expression.Append(" || ");
                }

                expressionTreeVisitor.VisitExpression(argument);
            }
            
            expression.Append(')');

            return methodCallExpression;
        }

        private IEnumerable<Expression> GetConcatenatedItems(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments.Count == 1
                && (typeof(IEnumerable).IsAssignableFrom(methodCallExpression.Arguments[0].Type)
                    && methodCallExpression.Arguments[0].Type != typeof(string)))
            {
                ConstantExpression argumentAsConstantExpression;
                NewArrayExpression argumentAsNewArrayExpression;

                if ((argumentAsNewArrayExpression = methodCallExpression.Arguments[0] as NewArrayExpression) != null)
                {
                    return argumentAsNewArrayExpression.Expressions;
                }
                else if ((argumentAsConstantExpression = methodCallExpression.Arguments[0] as ConstantExpression) != null)
                {
                    return ((object[])argumentAsConstantExpression.Value).Select(element => (Expression)Expression.Constant(element));
                }
                else
                {
                    var message = string.Format(
                        "The method call '{0}' is not supported. When the array overloads of String.Concat are used, only constant or new array expressions can "
                        + "be translated to SQL; in this usage, the expression has type '{1}'.",
                        methodCallExpression,
                        methodCallExpression.Arguments[0].GetType());
                    throw new NotSupportedException(message);
                }
            }
            else
            {
                return methodCallExpression.Arguments;
            }
        }
    }
}
