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

namespace Couchbase.Linq.UnitTests.QueryGeneration.MethodCallTranslators
{
    class ContainsMethodCallTranslatorTests
    {
        #region Translate

        [Test]
        public void Translate_NoMethod_ThrowsArgumentNullException()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var transformer = new ContainsMethodCallTranslator();

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => transformer.Translate(null, visitor.Object));

            Assert.AreEqual("methodCallExpression", result.ParamName);
        }

        [Test]
        public void Translate_NoVisitor_ThrowsArgumentNullException()
        {
            // Arrange

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var expression = Expression.Call(Expression.Constant("test"), method, Expression.Constant("t"));

            var transformer = new ContainsMethodCallTranslator();

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => transformer.Translate(expression, null));

            Assert.AreEqual("expressionTreeVisitor", result.ParamName);
        }

        [Test]
        public void Translate_ContainsConstant_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var expression = Expression.Call(Expression.Constant("test"), method, Expression.Constant("t"));

            var transformer = new ContainsMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("('test' LIKE '%t%')", result);
        }

        [Test]
        public void Translate_ContainsExpression_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };
            visitor.Setup(m => m.Visit(It.IsAny<ParameterExpression>())).Callback((Expression p) =>
            {
                visitor.Object.Expression.Append("FAKE");
            });

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var expression = Expression.Call(Expression.Constant("test"), method,
                Expression.Parameter(typeof(string)));

            var transformer = new ContainsMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("('test' LIKE '%' || FAKE || '%')", result);
        }

        [Test]
        public void Translate_StartsWithConstant_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var expression = Expression.Call(Expression.Constant("test"), method, Expression.Constant("t"));

            var transformer = new ContainsMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("('test' LIKE 't%')", result);
        }

        [Test]
        public void Translate_StartsWithExpression_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };
            visitor.Setup(m => m.Visit(It.IsAny<ParameterExpression>())).Callback((Expression p) =>
            {
                visitor.Object.Expression.Append("FAKE");
            });

            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var expression = Expression.Call(Expression.Constant("test"), method,
                Expression.Parameter(typeof(string)));

            var transformer = new ContainsMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("('test' LIKE FAKE || '%')", result);
        }

        [Test]
        public void Translate_EndsWithConstant_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});
            var expression = Expression.Call(Expression.Constant("test"), method, Expression.Constant("t"));

            var transformer = new ContainsMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("('test' LIKE '%t')", result);
        }

        [Test]
        public void Translate_EndsWithExpression_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };
            visitor.Setup(m => m.Visit(It.IsAny<ParameterExpression>())).Callback((Expression p) =>
            {
                visitor.Object.Expression.Append("FAKE");
            });

            var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            var expression = Expression.Call(Expression.Constant("test"), method,
                Expression.Parameter(typeof(string)));

            var transformer = new ContainsMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("('test' LIKE '%' || FAKE)", result);
        }

        #endregion
    }
}
