using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Converts any DateTime constants or properties to Unix milliseconds before comparing them.
    /// This way the <see cref="MethodCallTranslators.UnixMillisecondsMethodCallTranslator">UnixMillisecondsMethodCallTranslator</see>
    /// will later interpret the calls as STR_TO_MILLIS() calls in N1QL.  N1QL can't directly compare
    /// date/time values unless they're converted to Unix milliseconds first.
    /// </summary>
    internal class DateTimeComparisonExpressionTransformer : IExpressionTransformer<BinaryExpression>
    {
        private static readonly MethodInfo FromDateTimeMethod =
            typeof (UnixMillisecondsDateTime).GetMethod("FromDateTime", new[] { typeof(DateTime) });
        private static readonly MethodInfo FromDateTimeNullableMethod =
            typeof(UnixMillisecondsDateTime).GetMethod("FromDateTime", new[] { typeof(DateTime?) });

        public ExpressionType[] SupportedExpressionTypes
        {
            get
            {
                return new[]
                {
                    ExpressionType.GreaterThan,
                    ExpressionType.GreaterThanOrEqual,
                    ExpressionType.LessThan,
                    ExpressionType.LessThanOrEqual,
                    ExpressionType.Equal,
                    ExpressionType.NotEqual
                };
            }
        }

        public Expression Transform(BinaryExpression expression)
        {
            var constantExpression = expression.Right as ConstantExpression;
            if ((constantExpression != null) && (constantExpression.Value == null))
            {
                // Testing for null, so don't do transformation

                return expression;
            }

            var left = TransformSide(expression.Left);
            var right = TransformSide(expression.Right);

            if ((left != expression.Left) || (right != expression.Right))
            {
                return Expression.MakeBinary(expression.NodeType, left, right);
            }
            else
            {
                return expression;
            }
        }

        private Expression TransformSide(Expression side)
        {
            if (side.Type == typeof (DateTime))
            {
                return Expression.Call(FromDateTimeMethod, side);
            }
            else if (side.Type == typeof(DateTime?))
            {
                return Expression.Call(FromDateTimeNullableMethod, side);
            }
            else
            {
                return side;
            }
        }
    }
}
