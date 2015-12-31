using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration.Expressions;
using Couchbase.Linq.QueryGeneration.ExpressionTransformers;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration.ExpressionTransformers
{
    [TestFixture]
    class StringComparisonExpressionTransformerTests : N1QLTestBase
    {
        [Test]
        public void StringCompare_LessThan1_ReturnsLessThanOrEqual()
        {
            // Arrange

            var string1 = Expression.Constant("A");
            var string2 = Expression.Constant("B");

            BinaryExpression expression = Expression.LessThan(
                Expression.Call(
                    typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }),
                    string1,
                    string2
                ),
                Expression.Constant(1));

            var transformer = new StringComparisonExpressionTransformer();

            // Act

            var newExpression = transformer.Transform(expression) as StringComparisonExpression;

            // Assert

            Assert.NotNull(newExpression);
            Assert.AreEqual(ExpressionType.LessThanOrEqual, newExpression.Operation);
            Assert.AreEqual(string1, newExpression.Left);
            Assert.AreEqual(string2, newExpression.Right);
        }

        [Test]
        public void StringCompare_EqualTo1_ReturnsGreaterThan()
        {
            // Arrange

            var string1 = Expression.Constant("A");
            var string2 = Expression.Constant("B");

            BinaryExpression expression = Expression.Equal(
                Expression.Call(
                    typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }),
                    string1,
                    string2
                ),
                Expression.Constant(1));

            var transformer = new StringComparisonExpressionTransformer();

            // Act

            var newExpression = transformer.Transform(expression) as StringComparisonExpression;

            // Assert

            Assert.NotNull(newExpression);
            Assert.AreEqual(ExpressionType.GreaterThan, newExpression.Operation);
            Assert.AreEqual(string1, newExpression.Left);
            Assert.AreEqual(string2, newExpression.Right);
        }

        [Test]
        public void StringCompare_GreaterThanNeg1_ReturnsGreaterThanOrEqual()
        {
            // Arrange

            var string1 = Expression.Constant("A");
            var string2 = Expression.Constant("B");

            BinaryExpression expression = Expression.GreaterThan(
                Expression.Call(
                    typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }),
                    string1,
                    string2
                ),
                Expression.Constant(-1));

            var transformer = new StringComparisonExpressionTransformer();

            // Act

            var newExpression = transformer.Transform(expression) as StringComparisonExpression;

            // Assert

            Assert.NotNull(newExpression);
            Assert.AreEqual(ExpressionType.GreaterThanOrEqual, newExpression.Operation);
            Assert.AreEqual(string1, newExpression.Left);
            Assert.AreEqual(string2, newExpression.Right);
        }

        [Test]
        public void StringCompare_EqualToNeg1_ReturnsLessThan()
        {
            // Arrange

            var string1 = Expression.Constant("A");
            var string2 = Expression.Constant("B");

            BinaryExpression expression = Expression.Equal(
                Expression.Call(
                    typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }),
                    string1,
                    string2
                ),
                Expression.Constant(-1));

            var transformer = new StringComparisonExpressionTransformer();

            // Act

            var newExpression = transformer.Transform(expression) as StringComparisonExpression;

            // Assert

            Assert.NotNull(newExpression);
            Assert.AreEqual(ExpressionType.LessThan, newExpression.Operation);
            Assert.AreEqual(string1, newExpression.Left);
            Assert.AreEqual(string2, newExpression.Right);
        }
    }
}
