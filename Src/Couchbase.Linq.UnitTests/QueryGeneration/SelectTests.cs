using System;
using System.Linq;
using Couchbase.Core;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Couchbase.Linq.Versioning;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class SelectTests : N1QLTestBase
    {
        [Test]
        public void Test_Select_With_Projection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new {age = e.Age, name = e.FirstName});

            const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_WithStronglyTypedProjection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => new Contact() { Age = e.Age, FirstName = e.FirstName });

            const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `fname` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_WithUnixMillisecondsProjection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<UnixMillisecondsDocument>(mockBucket.Object)
                    .Select(e => new UnixMillisecondsDocument { DateTime = e.DateTime });

            // Since the source and dest are both using UnixMillisecondsConverter, no functions should be applied
            const string expected = "SELECT `Extent1`.`DateTime` as `DateTime` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_WithUnixMillisecondsToIsoProjection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<UnixMillisecondsDocument>(mockBucket.Object)
                    .Select(e => new IsoDocument { DateTime = e.DateTime });

            const string expected = "SELECT MILLIS_TO_STR(`Extent1`.`DateTime`) as `DateTime` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_WithIsoToUnixMillisecondsProjection()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<IsoDocument>(mockBucket.Object)
                    .Select(e => new UnixMillisecondsDocument { DateTime = e.DateTime });

            const string expected = "SELECT STR_TO_MILLIS(`Extent1`.`DateTime`) as `DateTime` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_All_Properties()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e);

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_All_Properties_Raw()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e);

            const string expected = "SELECT RAW `Extent1` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression, FeatureVersions.SelectRaw);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_Single_Property()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e.FirstName);

            const string expected = "SELECT `Extent1`.`fname` as `result` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_Single_Property_Raw()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .Select(e => e.FirstName);

            const string expected = "SELECT RAW `Extent1`.`fname` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression, FeatureVersions.SelectRaw);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Select_UseKeys()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query =
                QueryFactory.Queryable<Contact>(mockBucket.Object)
                    .UseKeys(new[] { "abc", "def" })
                    .Select(e => e);

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` USE KEYS ['abc', 'def']";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #region Helpers

        public class IsoDocument
        {
            public DateTime DateTime { get; set; }
        }

        public class UnixMillisecondsDocument
        {
            [JsonConverter(typeof(UnixMillisecondsConverter))]
            public DateTime DateTime { get; set; }
        }

        #endregion
    }
}