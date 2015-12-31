using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Recognizes <see cref="string.Compare(string, string)"/> method calls, and converts them to
    /// <see cref="Expressions.StringComparisonExpression"/> expressions.
    /// </summary>
    internal class StringComparisonExpressionTransformer : IExpressionTransformer<BinaryExpression>
    {
        private static readonly MethodInfo[] StringCompareMethods = {
           typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }),
           typeof(string).GetMethod("CompareTo", new[] { typeof(string) })
        };

        public ExpressionType[] SupportedExpressionTypes
        {
            get { return StringComparisonExpression.SupportedOperations; }
        }

        /// <summary>
        /// Converts <see cref="string.Compare(string, string)"/> expressions to a
        /// <see cref="Expressions.StringComparisonExpression"/>, if applicable
        /// </summary>
        /// <param name="expression">BinaryExpression to test and convert</param>
        /// <returns>If not a String.Compare expression, returns the original expression.  Otherwise the converted expression.</returns>
        /// <remarks>
        /// Converts String.Compare and String.CompareTo clauses where compared to an integer.
        /// i.e. String.Compare(x, y) &lt; 0 or x.CompareTo(y) &lt; 0 are both the equivalent of x &lt; y
        /// </remarks>
        public Expression Transform(BinaryExpression expression)
        {
            // See if one side is a call to String.Compare

            var leftExpression = expression.Left as MethodCallExpression;
            var rightExpression = expression.Right as MethodCallExpression;

            if ((leftExpression != null) && !StringCompareMethods.Contains(leftExpression.Method))
            {
                leftExpression = null;
            }
            if ((rightExpression != null) && !StringCompareMethods.Contains(rightExpression.Method))
            {
                rightExpression = null;
            }

            var methodCallExpression = leftExpression ?? rightExpression;

            if (methodCallExpression == null)
            {
                // Not a string comparison
                return expression;
            }

            // Get the number side of the comparison, which must be a constant integer

            var numericExpression = leftExpression != null
                ? expression.Right as ConstantExpression
                : expression.Left as ConstantExpression;

            if ((numericExpression == null) || !typeof(int).IsAssignableFrom(numericExpression.Type))
            {
                // Only convert if comparing to an integer
                return expression;
            }

            var number = (int)numericExpression.Value;

            // Get the strings from the method call parameters

            Expression leftString;
            Expression rightString;

            if (methodCallExpression.Arguments.Count > 1)
            {
                leftString = methodCallExpression.Arguments[0];
                rightString = methodCallExpression.Arguments[1];
            }
            else
            {
                leftString = methodCallExpression.Object;
                rightString = methodCallExpression.Arguments[0];
            }

            if (leftExpression == null)
            {
                // If the method call is on the right side of the binary expression, then reverse the strings

                var temp = leftString;
                leftString = rightString;
                rightString = temp;
            }

            return ConvertStringCompareExpression(leftString, rightString, expression.NodeType, number);
        }

        /// <summary>
        /// Converts String.Compare expression to StringComparisonExpression
        /// </summary>
        /// <param name="leftString">String expression on the left side of the comparison</param>
        /// <param name="rightString">String expression on the right side of the comparison</param>
        /// <param name="operation">Comparison operation being performed</param>
        /// <param name="number">Number that String.Compare was being compared to, typically 0, 1, or -1.</param>
        private Expression ConvertStringCompareExpression(Expression leftString, Expression rightString,
            ExpressionType operation, int number)
        {
            if (number == 1)
            {
                if ((operation == ExpressionType.LessThan) || (operation == ExpressionType.NotEqual))
                {
                    operation = ExpressionType.LessThanOrEqual;
                }
                else if ((operation == ExpressionType.Equal || operation == ExpressionType.GreaterThanOrEqual))
                {
                    operation = ExpressionType.GreaterThan;
                }
                else
                {
                    // Always evaluates to true or false regardless of input, so return a constant expression
                    return Expression.Constant(operation == ExpressionType.LessThanOrEqual,
                        typeof(bool));
                }
            }
            else if (number == -1)
            {
                if ((operation == ExpressionType.GreaterThan) || (operation == ExpressionType.NotEqual))
                {
                    operation = ExpressionType.GreaterThanOrEqual;
                }
                else if ((operation == ExpressionType.Equal || operation == ExpressionType.LessThanOrEqual))
                {
                    operation = ExpressionType.LessThan;
                }
                else
                {
                    // Always evaluates to true or false regardless of input, so return a constant expression
                    return Expression.Constant(operation == ExpressionType.GreaterThanOrEqual, typeof(bool));
                }
            }
            else if (number > 1)
            {
                // Always evaluates to true or false regardless of input, so return a constant expression
                return Expression.Constant(
                    (operation == ExpressionType.NotEqual) || (operation == ExpressionType.LessThan) || (operation == ExpressionType.LessThanOrEqual),
                    typeof(bool));
            }
            else if (number < -1)
            {
                // Always evaluates to true or false regardless of input, so return a constant expression
                return Expression.Constant(
                    (operation == ExpressionType.NotEqual) || (operation == ExpressionType.GreaterThan) || (operation == ExpressionType.GreaterThanOrEqual),
                    typeof(bool));
            }

            // If number == 0 we just leave operation unchanged

            return StringComparisonExpression.Create(operation, leftString, rightString);
        }
    }
}
