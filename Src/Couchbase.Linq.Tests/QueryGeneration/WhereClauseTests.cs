using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    public class WhereClauseTests : N1QLTestBase
    {
        [Test]
        public void Test_Where_With_OrderBy()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .OrderBy(e => e.Age)
                    .Select(e => new { age = e.Age, name = e.FirstName });


            const string expected = "SELECT e.age, e.name FROM default as e WHERE (((e.Age > 10) AND (e.FirstName = Sam)) ORDER BY e.Age";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Where_With_Like()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam" && e.LastName.Contains("a"))
                    .Select(e => new { age = e.Age, name = e.FirstName });



            const string expected = "SELECT e.age, e.name FROM default as e WHERE (((e.Age > 10) AND (e.FirstName = Sam)) AND (e.LastName LIKE '%a%'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }


        [Test]
        public void Test_Where()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .Select(e => new { age = e.Age, name = e.FirstName });


            const string expected = "SELECT e.age, e.name FROM default as e WHERE ((e.Age > 10) AND (e.FirstName = 'Sam'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }


        [Test]
        public void Test_Where_With_Parameters()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            const int age = 10;
            const string firstName = "Sam";

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > age && e.FirstName == firstName)
                    .Select(e => new { age = e.Age, name = e.FirstName });


            const string expected = "SELECT e.age, e.name FROM default as e WHERE ((e.Age > 10) AND (e.FirstName = 'Sam'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }



        [Test]
        public void Test_Multiple_Where()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .Where(e => e.Email == "myemail@test.com")
                    .Select(e => new { age = e.Age, name = e.FirstName });


            const string expected = "SELECT e.age, e.name FROM default as e WHERE ((e.Age > 10) AND (e.FirstName = Sam))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);

        }

    }
}
