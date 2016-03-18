using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.MethodCallTranslators;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration.MethodCallTranslators
{
    class KeyMethodCallTranslatorTests
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

            var transformer = new KeyMethodCallTranslator();

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => transformer.Translate(null, visitor.Object));

            Assert.AreEqual("methodCallExpression", result.ParamName);
        }

        [Test]
        public void Translate_NoVisitor_ThrowsArgumentNullException()
        {
            // Arrange

            var method = typeof(N1QlFunctions).GetMethod("Key");
            var expression = Expression.Call(method,
                Expression.Constant("arg1"));

            var transformer = new KeyMethodCallTranslator();

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => transformer.Translate(expression, null));

            Assert.AreEqual("expressionTreeVisitor", result.ParamName);
        }

        [Test]
        public void Translate_OneParameter_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(N1QlFunctions).GetMethod("Key");
            var expression = Expression.Call(method,
                Expression.Constant("arg1"));

            var transformer = new KeyMethodCallTranslator();

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("META('arg1').id", result);
        }

        #endregion
    }
}
