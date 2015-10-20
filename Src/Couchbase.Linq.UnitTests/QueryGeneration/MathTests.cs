using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    class MathTests : N1QLTestBase
    {

        #region Algebraic

        [Test]
        public void Test_Abs()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Abs(contact.Age) };

            const string expected =
                "SELECT ABS(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Ceiling()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Ceiling((double)contact.Age) };

            const string expected =
                "SELECT CEIL(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Floor()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Floor((double)contact.Age) };

            const string expected =
                "SELECT FLOOR(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Power()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Pow(contact.Age, 2) };

            const string expected =
                "SELECT POWER(`Extent1`.`age`, 2) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Round()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Round((double)contact.Age) };

            const string expected =
                "SELECT ROUND(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Round_Digits()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Round((double)contact.Age, 2) };

            const string expected =
                "SELECT ROUND(`Extent1`.`age`, 2) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Sign()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Sign(contact.Age) };

            const string expected =
                "SELECT SIGN(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Sqrt()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Sqrt(contact.Age) };

            const string expected =
                "SELECT SQRT(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Truncate()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Truncate((double)contact.Age) };

            const string expected =
                "SELECT TRUNC(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Logarithmic

        [Test]
        public void Test_Exp()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Exp(contact.Age) };

            const string expected =
                "SELECT EXP(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Log()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Log(contact.Age) };

            const string expected =
                "SELECT LN(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Log10()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Log10(contact.Age) };

            const string expected =
                "SELECT LOG(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Trigonometric

        [Test]
        public void Test_Acos()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Acos(contact.Age) };

            const string expected =
                "SELECT ACOS(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Asin()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Asin(contact.Age) };

            const string expected =
                "SELECT ASIN(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Atan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Atan(contact.Age) };

            const string expected =
                "SELECT ATAN(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Atan2()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Atan2(contact.Age, 2) };

            const string expected =
                "SELECT ATAN2(`Extent1`.`age`, 2) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Cos()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Cos(contact.Age) };

            const string expected =
                "SELECT COS(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Sin()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Sin(contact.Age) };

            const string expected =
                "SELECT SIN(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Tan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { i = Math.Tan(contact.Age) };

            const string expected =
                "SELECT TAN(`Extent1`.`age`) as `i` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion
        
    }
}
