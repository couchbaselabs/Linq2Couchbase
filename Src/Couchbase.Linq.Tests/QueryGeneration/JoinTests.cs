using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.QueryGeneration
{
    [TestFixture]
    public class JoinTests : N1QLTestBase
    {
        [Test]
        public void Test_Explain_Keyword()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from p in QueryFactory.Queryable<Contact>(mockBucket.Object)
                join c in QueryFactory.Queryable<Child>(mockBucket.Object)
                        on p.FirstName equals c.FirstName
                select new {p.LastName, c.Age};

            const string expected = "SELECT p.lastname, c.age " +
                "FROM default as p "+
                "JOIN default as c " +
                "ON KEYS ARRAY s.contactId FOR s IN p END";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }
    }
}
