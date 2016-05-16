using System;
using System.Linq;
using Couchbase.Linq.IntegrationTests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    /// <summary>
    /// These tests don't necessary represent "good practices" in general use
    /// (e.g. concatenation instead of parameterization, using N1QL update on a single document)
    /// </summary>
    [TestFixture]
    public class BucketDataStoreTests
    {
        [Test]
        public void Can_be_used_to_execute_N1QL_query_directly()
        {
            // setup bucket context
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            // get a beer from the db
            var beer = db.Query<Beer>().Select(b => N1QlFunctions.Meta(b)).First();

            // execute N1QL to update the beer
            var newBeerName = Guid.NewGuid().ToString();
            var n1ql = string.Format("UPDATE `beer-sample` USE KEYS '{0}' SET name = '{1}';", beer.Id, newBeerName);
            db.DataStore.Execute<dynamic>(n1ql);

            // get the beer back out to make sure it was updated correctly
            var beerAgain = db.Query<Beer>().First(b => N1QlFunctions.Meta(b).Id == beer.Id);

            Assert.That(beerAgain.Name, Is.EqualTo(newBeerName));
        }

        [Test]
        public void Can_be_used_to_execute_N1QL_query_directly_with_parameterization()
        {
            // setup bucket context
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            // get a beer from the db
            var beer = db.Query<Beer>().Select(b => N1QlFunctions.Meta(b)).First();

            // execute N1QL to update the beer, use parameterization instead of string.Format
            var newBeerName = Guid.NewGuid().ToString();
            var n1ql = "UPDATE `beer-sample` USE KEYS $1 SET name = $2;";
            db.DataStore.Execute<dynamic>(n1ql, beer.Id, newBeerName);

            // get the beer back out to make sure it was updated correctly
            var beerAgain = db.Query<Beer>().First(b => N1QlFunctions.Meta(b).Id == beer.Id);

            Assert.That(beerAgain.Name, Is.EqualTo(newBeerName));
        }
    }
}