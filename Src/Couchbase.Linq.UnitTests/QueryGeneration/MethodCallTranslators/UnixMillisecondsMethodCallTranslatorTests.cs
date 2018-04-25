using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Core.Serialization;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.Linq.QueryGeneration.MethodCallTranslators;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Couchbase.Linq.UnitTests.QueryGeneration.MethodCallTranslators
{
    class UnixMillisecondsMethodCallTranslatorTests
    {
        private static readonly DateTime ExampleDateTime = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string ExampleDateTimeString = "2010-01-01T00:00:00Z";
        private const long ExampleDateTimeUnixMilliseconds = 1262304000000;

        #region Translate

        [Test]
        public void Translate_NoMethod_ThrowsArgumentNullException()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var translator = new UnixMillisecondsMethodCallTranslator();

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => translator.Translate(null, visitor.Object));

            Assert.AreEqual("methodCallExpression", result.ParamName);
        }

        [Test]
        public void Translate_NoVisitor_ThrowsArgumentNullException()
        {
            // Arrange

            var method = typeof(UnixMillisecondsDateTime).GetTypeInfo().GetMethod("FromDateTime", new [] {typeof(DateTime)});
            Assert.NotNull(method);

            var expression = Expression.Call(method,
                Expression.Constant(ExampleDateTime));

            var translator = new UnixMillisecondsMethodCallTranslator();

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => translator.Translate(expression, null));

            Assert.AreEqual("expressionTreeVisitor", result.ParamName);
        }

        [Test]
        public void Translate_FromDateTimeConstant_RendersCorrectly()
        {
            // Arrange

            var serializer = new DefaultSerializer();

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext()
            {
                Serializer = serializer,
                MemberNameResolver = new ExtendedTypeSerializerMemberNameResolver(serializer)
            })
            {
                CallBase = true
            };

            var method = typeof(UnixMillisecondsDateTime).GetTypeInfo().GetMethod("FromDateTime", new[] { typeof(DateTime) });
            Assert.NotNull(method);

            var expression = Expression.Call(method,
                Expression.Constant(ExampleDateTime));

            var translator = new UnixMillisecondsMethodCallTranslator();

            // Act

            translator.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual($"STR_TO_MILLIS(\"{ExampleDateTimeString}\")", result);
        }

        [Test]
        public void Translate_FromDateTimeIso_RendersCorrectly()
        {
            // Arrange

            var serializer = new DefaultSerializer();
            var extentNameProvider = new N1QlExtentNameProvider();

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext
            {
                Serializer = serializer,
                MemberNameResolver = new ExtendedTypeSerializerMemberNameResolver(serializer),
                ExtentNameProvider = extentNameProvider
            })
            {
                CallBase = true
            };

            var method = typeof(UnixMillisecondsDateTime).GetTypeInfo().GetMethod("FromDateTime", new[] { typeof(DateTime) });
            Assert.NotNull(method);

            var property = typeof(Iso).GetTypeInfo().GetProperty("Value");
            Assert.NotNull(property);

            var querySource = new Mock<IQuerySource>();
            querySource.SetupGet(m => m.ItemName).Returns("p");
            querySource.SetupGet(m => m.ItemType).Returns(typeof(Iso));

            var querySourceReference = new QuerySourceReferenceExpression(querySource.Object);

            var expression = Expression.Call(method,
                Expression.MakeMemberAccess(
                    querySourceReference,
                    property));

            var translator = new UnixMillisecondsMethodCallTranslator();

            // Act

            translator.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            var extentName = extentNameProvider.GetExtentName(querySource.Object);
            Assert.AreEqual($"STR_TO_MILLIS({extentName}.`value`)", result);
        }

        [Test]
        public void Translate_FromDateTimeUnix_RendersCorrectly()
        {
            // Arrange

            var serializer = new DefaultSerializer();
            var extentNameProvider = new N1QlExtentNameProvider();

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext
            {
                Serializer = serializer,
                MemberNameResolver = new ExtendedTypeSerializerMemberNameResolver(serializer),
                ExtentNameProvider = extentNameProvider
            })
            {
                CallBase = true
            };

            var method = typeof(UnixMillisecondsDateTime).GetTypeInfo().GetMethod("FromDateTime", new[] { typeof(DateTime) });
            Assert.NotNull(method);

            var property = typeof(UnixMillis).GetTypeInfo().GetProperty("Value");
            Assert.NotNull(property);

            var querySource = new Mock<IQuerySource>();
            querySource.SetupGet(m => m.ItemName).Returns("p");
            querySource.SetupGet(m => m.ItemType).Returns(typeof(UnixMillis));

            var querySourceReference = new QuerySourceReferenceExpression(querySource.Object);

            var expression = Expression.Call(method,
                Expression.MakeMemberAccess(
                    querySourceReference,
                    property));

            var translator = new UnixMillisecondsMethodCallTranslator();

            // Act

            translator.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            var extentName = extentNameProvider.GetExtentName(querySource.Object);
            Assert.AreEqual($"{extentName}.`value`", result);
        }

        #endregion

        #region Helpers

        private class Iso
        {
            public DateTime Value { get; set; }
        }

        private class UnixMillis
        {
            [JsonConverter(typeof(UnixMillisecondsConverter))]
            public DateTime Value { get; set; }
        }

        #endregion
    }
}
