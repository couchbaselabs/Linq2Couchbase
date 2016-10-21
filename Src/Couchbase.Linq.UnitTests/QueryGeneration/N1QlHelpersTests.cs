using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using NUnit.Framework;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    class N1QlHelpersTests
    {

        [TestCase("bucket", "`bucket`")]
        [TestCase("some-bucket", "`some-bucket`")]
        [TestCase("some`bucket", "`some``bucket`")]
        public void EscapeIdentifier_WrapsSuccessfully(string identifier, string expectedResult)
        {
            var result = N1QlHelpers.EscapeIdentifier(identifier);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void EscapeIdentifier_Null_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => N1QlHelpers.EscapeIdentifier(null));
        }

        [TestCase("index")]
        [TestCase("INDEX")]
        public void IsValidKeyword_ReturnsTrue(string identifier)
        {
            var result = N1QlHelpers.IsValidKeyword(identifier);

            Assert.IsTrue(result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("ABC1")]
        [TestCase("a-b")]
        [TestCase("`ABC`")]
        [TestCase("/*ABC*/")]
        [TestCase("TWO WORDS")]
        public void IsValidKeyword_ReturnsFalse(string identifier)
        {
            var result = N1QlHelpers.IsValidKeyword(identifier);

            Assert.IsFalse(result);
        }
    }
}
