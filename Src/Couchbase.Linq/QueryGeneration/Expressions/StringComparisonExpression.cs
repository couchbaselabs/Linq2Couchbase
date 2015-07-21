using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.Expressions
{
    /// <summary>
    /// Represents a comparison between two strings
    /// </summary>
    public class StringComparisonExpression : Expression
    {

        public static readonly ExpressionType[] SupportedOperations =
        {
            ExpressionType.Equal,
            ExpressionType.NotEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual
        };

        public ExpressionType Operation { get; private set; }
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        public static StringComparisonExpression Create(ExpressionType operation, Expression left, Expression right)
        {
            if (!SupportedOperations.Contains(operation))
            {
                throw new ArgumentOutOfRangeException("operation");
            }
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            return new StringComparisonExpression(operation, left, right);
        }

        private StringComparisonExpression(ExpressionType operation, Expression left, Expression right)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}",
                Left,
                Operation,
                Right);
        }
    }
}
