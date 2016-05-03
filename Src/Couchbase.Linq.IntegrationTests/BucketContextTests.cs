using System;
using System.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Proxies;
using NUnit.Framework;

namespace Couchbase.Linq.IntegrationTests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class BucketContextTests
    {
        [Test]
        public void Test_Basic_Query()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));
            var query = from x in db.Query<Beer>()
                where x.Type == "beer"
                select x;

            var beer = query.FirstOrDefault();
            Assert.IsNotNull(beer);
        }

        [Test]
        public void BeerSampleContext_Tests()
        {
            var db = new BeerSample();
            var beers = from b in db.Beers
                select b;

            var beer = beers.Take(1);
            Assert.IsNotNull(beer);
        }

        [Test]
        public void BeerSample_Tests()
        {
            var db = new BeerSample();
            var query = from beer in db.Beers
                        join brewery in db.Breweries
                        on beer.BreweryId equals N1QlFunctions.Key(brewery)
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            foreach (var beer in query.Take(1))
            {
                Console.WriteLine(beer.Name);
            }
        }

        #region Proxies

        [Test]
        public void Query_EnableProxyGeneration_ReturnsProxyBeer()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Beer>()
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.IsFalse(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGeneration_ReturnsProxyBeerWithId()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));
            db.BeginChangeTracking();

            const string documentId = "21st_amendment_brewery_cafe-21a_ipa";

            var query = from x in db.Query<Beer>().UseKeys(new[] { documentId })
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.AreEqual(documentId, status.Metadata.Id);
        }

        [Test]
        public void Query_EnableProxyGeneration_ReturnsProxyBeerWithCas()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));
            db.BeginChangeTracking();

            const string documentId = "21st_amendment_brewery_cafe-21a_ipa";

            var query = from x in db.Query<Beer>().UseKeys(new[] { documentId })
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.Greater(status.Metadata.Cas, 0);
        }

        [Test]
        public void Query_DisableProxyGeneration_ReturnsNoProxy()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            const string documentId = "21st_amendment_brewery_cafe-21a_ipa";

            var query = from x in db.Query<Beer>().UseKeys(new[] { documentId })
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.Null(status);
        }

        [Test]
        public void Query_EnableProxyGenerationChanges_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Beer>()
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.False(status.IsDirty);

            beer.Name = "New Name";

            Assert.True(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGenerationChangesInSubDocuments_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Brewery>()
                        where x.Type == "brewery" && N1QlFunctions.IsValued(x.Geo)
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.False(status.IsDirty);

            beer.Geo.Latitude = 90M;

            Assert.True(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGeneration_ReturnsProxyBreweryAddressCollection()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Brewery>()
                        where x.Type == "brewery"
                        select x;

            var brewery = query.First();
            var addresses = brewery.Address;

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = addresses as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.IsFalse(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGenerationClearAddresses_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();


            var query = from x in db.Query<Brewery>()
                        where x.Type == "brewery" && x.Address.Any()
                        select x;

            var brewery = query.First();
            var addresses = brewery.Address;

            addresses.Clear();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = addresses as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.IsTrue(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGenerationAddAddress_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Brewery>()
                        where x.Type == "brewery" && x.Address.Any()
                        select x;

            var brewery = query.First();
            var addresses = brewery.Address;

            addresses.Add("Test");

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = addresses as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.IsTrue(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGenerationRemoveAddress_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Brewery>()
                        where x.Type == "brewery" && x.Address.Any()
                        select x;

            var brewery = query.First();
            var addresses = brewery.Address;

            addresses.RemoveAt(0);

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = addresses as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.IsTrue(status.IsDirty);
        }

        [Test]
        public void Query_EnableProxyGenerationSetAddress_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Brewery>()
                        where x.Type == "brewery" && x.Address.Any()
                        select x;

            var brewery = query.First();
            var addresses = brewery.Address;

            addresses[0] = "Test";

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = addresses as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.IsTrue(status.IsDirty);
        }

        #endregion

        [Test]
        public void BeginChangeTracking_DoesNotClear_Modified_List()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            db.BeginChangeTracking();

            var query = from x in db.Query<Beer>()
                        where x.Type == "beer"
                        select x;

            db.BeginChangeTracking();

            var context = db as IChangeTrackableContext;

            Assert.AreEqual(0, context.ModifiedCount);

            var brewery = query.First();
            brewery.Abv = 10;

            Assert.AreEqual(1, context.ModifiedCount);

            db.BeginChangeTracking();

            Assert.AreEqual(1, context.ModifiedCount);
        }


        [Test]
        public void SubmitChanges_WhenDocsAreModified_DocumentIsMutated()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            var query = from x in db.Query<Beer>()
                        where x.Type == "beer"
                        select x;

            db.BeginChangeTracking();

            var beer = query.First();

            beer.Abv = beer.Abv+1;

            db.SubmitChanges();

            var doc = ClusterHelper.GetBucket("beer-sample").GetDocument<Beer>(((ITrackedDocumentNode) beer).Metadata.Id);
            Assert.AreEqual(beer.Abv, doc.Content.Abv);
        }

        [Test]
        public void SubmitChanges_WhenDocsAreModifiedAndEndChangeTracking_DocumentIsNotMutated()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            var query = from x in db.Query<Beer>()
                        where x.Type == "beer"
                        select x;

            db.BeginChangeTracking();

            var beer = query.First();

            beer.Abv = beer.Abv + 1;

            db.EndChangeTracking();

            db.SubmitChanges();

            var doc = ClusterHelper.GetBucket("beer-sample").GetDocument<Beer>(((ITrackedDocumentNode)beer).Metadata.Id);
            Assert.AreNotEqual(beer.Abv, doc.Content.Abv);
        }

        [Test]
        public void Underlying_Bucket_can_be_used_to_execute_N1QL_query_directly()
        {
            // setup bucket context
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));

            // get a beer from the db
            var beer = db.Query<Beer>().Select(b => N1QlFunctions.Meta(b)).First();

            // execute N1QL to update the beer
            var newBeerName = Guid.NewGuid().ToString();
            var n1ql = string.Format("UPDATE `beer-sample` USE KEYS '{0}' SET name = '{1}';", beer.Id, newBeerName);
            db.Bucket.Query<int>(n1ql);

            // get the beer back out to make sure it was updated correctly
            var beerAgain = db.Query<Beer>().First(b => N1QlFunctions.Meta(b).Id == beer.Id);

            Assert.That(beerAgain.Name, Is.EqualTo(newBeerName));
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2015 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
