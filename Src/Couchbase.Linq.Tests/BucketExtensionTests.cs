using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.QueryGeneration;
using NUnit.Framework;
using Couchbase.Linq;
using Couchbase.Linq.Tests.Documents;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq.Tests
{
    [TestFixture]
    public class BucketExtensionTests
    {
        [Test]
        public void Test_AnonymousType_In_Projection()
        {

            using (var cluster = new CouchbaseCluster())
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in QueryFactory.Queryable<Contact>(bucket)
                        select new 
                        {
                            age = c.Age, 
                            fname = c.FirstName
                        };

                    const string expected = "SELECT c.age, c.fname FROM default as c";
                    Assert.AreEqual(expected, CreateN1QlQuery(bucket, query.Expression));
                }
            }
        }

        [Test]
        public void Test_POCO()
        {
            using (var cluster = new CouchbaseCluster())
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in bucket.Queryable<Contact>()
                        select c;

                    const string expected = "SELECT c.* FROM default as c";
                    Assert.AreEqual(expected, CreateN1QlQuery(bucket, query.Expression));
                }
            }
        }

        [Test]
        public void Test_Select_Children()
        {
            using (var cluster = new CouchbaseCluster())
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in bucket.Queryable<Contact>()
                        select c.Children;

                    const string expected = "SELECT c.children FROM default as c";
                    Assert.AreEqual(expected, CreateN1QlQuery(bucket, query.Expression));
                }
            }
        }

        private string CreateN1QlQuery(IBucket bucket, Expression expression)
        {
            var queryModel = QueryParser.CreateDefault().GetParsedQuery(expression);
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel, bucket.Name);
        }

        [Test]
        public void Test()
        {
            using (var cluster = new CouchbaseCluster())
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var query = from c in bucket.Queryable<Contact>()
                        select c;

                    foreach (var contact in query)
                    {
                        if (contact.Hobbies != null)
                            foreach (var hobby in contact.Hobbies)
                            {
                                Console.WriteLine(hobby);
                            }
                    }
                }
            }
        }

        [Test]
        public void Test_POCO_Basic()
        {
            using (var cluster = new CouchbaseCluster())
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                }
            }
        }
    }
}
