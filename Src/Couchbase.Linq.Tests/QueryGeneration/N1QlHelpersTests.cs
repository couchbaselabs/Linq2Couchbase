using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using NUnit.Framework;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Couchbase.Linq.Tests.QueryGeneration
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

    }
}
