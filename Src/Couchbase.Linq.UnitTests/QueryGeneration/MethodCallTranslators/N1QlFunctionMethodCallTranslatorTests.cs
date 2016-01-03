using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    class N1QlFunctionMethodCallTranslatorTests
    {
        #region Constructor

        [Test]
        public void Constructor_NoMethod_ThrowsArgumentNullException()
        {
            // Act/Assert

            // ReSharper disable once ObjectCreationAsStatement
            var result = Assert.Throws<ArgumentNullException>(() => new N1QlFunctionMethodCallTranslator(null, new N1QlFunctionAttribute("FUNC")));

            Assert.AreEqual("methodInfo", result.ParamName);
        }

        [Test]
        public void Constructor_InstanceMethod_ThrowsArgumentException()
        {
            // Arrange

            var method = typeof (string).GetMethod("Clone");

            // Act/Assert

            // ReSharper disable once ObjectCreationAsStatement
            var result = Assert.Throws<ArgumentException>(() => new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC")));

            Assert.AreEqual("methodInfo", result.ParamName);
        }

        [Test]
        public void Constructor_NoAttribute_ThrowsArgumentNullException()
        {
            // Arrange

            var method = typeof(Methods).GetMethod("Method0");

            // Act/Assert

            // ReSharper disable once ObjectCreationAsStatement
            var result = Assert.Throws<ArgumentNullException>(() => new N1QlFunctionMethodCallTranslator(method, null));

            Assert.AreEqual("attribute", result.ParamName);
        }

        #endregion

        #region Properties

        [Test]
        public void SupportedMethods_ReturnsMethodInfoFromConstructor()
        {
            // Arrange

            var method = typeof(Methods).GetMethod("Method0");

            var translator = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act

            var result = new List<MethodInfo>(translator.SupportMethods);

            // Assert

            Assert.Contains(method, result);
        }

        #endregion

        #region Translate

        [Test]
        public void Translate_NoMethod_ThrowsArgumentNullException()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(Methods).GetMethod("Method0");

            var transformer = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => transformer.Translate(null, visitor.Object));

            Assert.AreEqual("methodCallExpression", result.ParamName);
        }

        [Test]
        public void Translate_MismatchedMethod_ThrowsArgumentException()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(Methods).GetMethod("Method1");
            var expression = Expression.Call(typeof(Methods).GetMethod("Method0"));

            var transformer = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act/Assert

            var result = Assert.Throws<ArgumentException>(() => transformer.Translate(expression, visitor.Object));

            Assert.AreEqual("methodCallExpression", result.ParamName);
        }

        [Test]
        public void Translate_NoVisitor_ThrowsArgumentNullException()
        {
            // Arrange

            var method = typeof(Methods).GetMethod("Method0");
            var expression = Expression.Call(method);

            var transformer = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act/Assert

            var result = Assert.Throws<ArgumentNullException>(() => transformer.Translate(expression, null));

            Assert.AreEqual("expressionTreeVisitor", result.ParamName);
        }

        [Test]
        public void Translate_NoParameters_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof (Methods).GetMethod("Method0");
            var expression = Expression.Call(method);

            var transformer = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("FUNC()", result);
        }

        [Test]
        public void Translate_OneParameter_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof(Methods).GetMethod("Method1");
            var expression = Expression.Call(method,
                Expression.Constant("arg1"));

            var transformer = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("FUNC('arg1')", result);
        }

        [Test]
        public void Translate_TwoParameters_RendersCorrectly()
        {
            // Arrange

            var visitor = new Mock<N1QlExpressionTreeVisitor>(new N1QlQueryGenerationContext())
            {
                CallBase = true
            };

            var method = typeof (Methods).GetMethod("Method2");
            var expression = Expression.Call(method,
                Expression.Constant("arg1"),
                Expression.Constant("arg2"));

            var transformer = new N1QlFunctionMethodCallTranslator(method, new N1QlFunctionAttribute("FUNC"));

            // Act

            transformer.Translate(expression, visitor.Object);
            var result = visitor.Object.GetN1QlExpression();

            // Assert

            Assert.AreEqual("FUNC('arg1', 'arg2')", result);
        }

        #endregion

        #region Helpers

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private class Methods
        {
            public static string Method0()
            {
                throw new NotImplementedException();
            }

            public static string Method1(string param1)
            {
                throw new NotImplementedException();
            }

            public static string Method2(string param1, string param2)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
