using System;
using System.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.Tests
{
    [TestFixture]
    public class BucketExtensionTests : N1QLTestBase
    {
        protected override bool IsClusterRequired
        {
            get { return true; }
        }

        [Test]
        public void Test_AnonymousType_In_Projection()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in QueryFactory.Queryable<Contact>(bucket)
                        select new
                        {
                            age = c.Age,
                            fname = c.FirstName
                        };

                    const string expected = "SELECT `Extent1`.`age` as `age`, `Extent1`.`fname` as `fname` FROM `default` as `Extent1`";

                    var N1QLQuery = CreateN1QlQuery(bucket, query.Expression);

                    Assert.AreEqual(expected, N1QLQuery);
                }
            }
        }

        [Test]
        public void Test_POCO()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in bucket.Queryable<Contact>()
                        select c;

                    const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1`";
                    Assert.AreEqual(expected, CreateN1QlQuery(bucket, query.Expression));
                }
            }
        }

        [Test]
        public void Test_Select_Children()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in bucket.Queryable<Contact>()
                        select c.Children;

                    const string expected = "SELECT `Extent1`.`children` FROM `default` as `Extent1`";
                    Assert.AreEqual(expected, CreateN1QlQuery(bucket, query.Expression));
                }
            }
        }

        [Test]
        public void Test_POCO_Basic()
        {
            using (var cluster = new Cluster(TestConfigurations.DefaultConfig()))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                }
            }
        }
    }
}