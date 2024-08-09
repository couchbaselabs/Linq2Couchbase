using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MethodCallTranslators;
using Moq;
using NUnit.Framework;
using System.Reflection;
using Couchbase.Core.IO.Serializers;

namespace Couchbase.Linq.UnitTests.QueryGeneration.MethodCallTranslators
{
    class DateMethodCallTranslatorTests
    {
        #region Translate

        [Test]
        public void Translate_DateTime_Date_RendersCorrectly()
        {
            // Arrange

            var queryGenerationContext = new N1QlQueryGenerationContext
            {
                Serializer = new DefaultSerializer()
            };

            var visitor = new Mock<N1QlExpressionTreeVisitor>(queryGenerationContext)
            {
                CallBase = true
            };

            var dateTime = new DateTime(2024, 01, 02, 3, 45, 10, DateTimeKind.Utc);

            var method = typeof(DateTime).GetProperty("Date").GetGetMethod();
            var expression = Expression.Call(Expression.Constant(dateTime), method);

            var transformer = new DateMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("DATE_TRUNC_STR(\"2024-01-02T03:45:10Z\",\"day\")", result);
        }

        [Test]
        public void Translate_DateTimeOffset_Date_RendersCorrectly()
        {
            // Arrange

            var queryGenerationContext = new N1QlQueryGenerationContext
            {
                Serializer = new DefaultSerializer()
            };

            var visitor = new Mock<N1QlExpressionTreeVisitor>(queryGenerationContext)
            {
                CallBase = true
            };

            var dateTime = new DateTimeOffset(2024, 01, 02, 3, 45, 10, TimeSpan.Zero);

            var method = typeof(DateTimeOffset).GetProperty("Date").GetGetMethod();
            var expression = Expression.Call(Expression.Constant(dateTime), method);

            var transformer = new DateMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("DATE_TRUNC_STR(\"2024-01-02T03:45:10+00:00\",\"day\")", result);
        }

        #endregion
    }
}
