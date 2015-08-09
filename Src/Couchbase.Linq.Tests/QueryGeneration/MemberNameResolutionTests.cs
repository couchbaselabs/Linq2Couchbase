using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Tests.Documents;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    public class MemberNameResolutionTests : N1QLTestBase
    {
        [Test]
        public void Test_Default_JsonProp_Att()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Where(e => e.Age < 10 + 30 && e.FirstName.Contains("a"))
                    .Select(
                        e => new {age = e.Age, firstName = e.FirstName, lastName = e.LastName, children = e.Children});

            const string expected =
                "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `firstName`, `Extent1`.`lname` as `lastName`, `Extent1`.`children` as `children` FROM `default` as `Extent1` WHERE ((`Extent1`.`age` < 40) AND (`Extent1`.`fname` LIKE '%a%'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Default_No_Att()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Brewery>(mockBucket.Object)
                    .Where(e => e.Country.Contains("a"))
                    .Select(e => new {brewName = e.Name, brewCity = e.City});

            const string expected =
                "SELECT `Extent1`.`name` as `brewName`, `Extent1`.`city` as `brewCity` FROM `default` as `Extent1` WHERE (`Extent1`.`country` LIKE '%a%')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Camel_No_Att()
        {
            SetContractResolver(new CamelCasePropertyNamesContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Brewery>(mockBucket.Object)
                    .Where(e => e.Country.Contains("a"))
                    .Select(e => new {brewName = e.Name, brewCity = e.City});

            const string expected =
                "SELECT `Extent1`.`name` as `brewName`, `Extent1`.`city` as `brewCity` FROM `default` as `Extent1` WHERE (`Extent1`.`country` LIKE '%a%')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Default_DataContract_With_Excluded_Member()
        {
            SetContractResolver(new DefaultContractResolver());

            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<ChildWithContract>(mockBucket.Object)
                    .Where(e => e.Age > 6)
                    .Select(e => new {name = e.FirstName, gender = e.Gender});

            const string expected = "SELECT `Extent1`.`fname` as `name` FROM `default` as `Extent1` WHERE (`Extent1`.`age` > 6)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}