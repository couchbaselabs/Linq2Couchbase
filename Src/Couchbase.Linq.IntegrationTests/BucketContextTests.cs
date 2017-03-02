using System;
using System.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Filters;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Proxies;
using Couchbase.Linq.Versioning;
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
        public void Query_EnableProxyGenerationAndStreaming_ReturnsProxyBeerWithCas()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"));
            db.BeginChangeTracking();

            const string documentId = "21st_amendment_brewery_cafe-21a_ipa";

            var query = from x in db.Query<Beer>().UseStreaming(true).UseKeys(new[] { documentId })
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

        #region Consistentency Tests

        [Test]
        public void Save_ThenQuery_ReturnsChanges()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.ReadYourOwnWrite)
            {
                Assert.Ignore("Cluster does not support RYOW, test skipped.");
            }

            var db = new BucketContext(bucket);
            var testValue = new Random().Next(0, 100000);
            var testKey = "Save_ThenQuery_ReturnsChanges_" + testValue;

            var testDocument = new Sample
            {
                Id = testKey,
                Value = testValue
            };

            db.Save(testDocument);

            try
            {
                Assert.NotNull(db.MutationState);

                var result = db.Query<Sample>()
                    .ConsistentWith(db.MutationState)
                    .FirstOrDefault(p => p.Id == testKey);

                Assert.NotNull(result);
                Assert.AreEqual(testValue, result.Value);
            }
            finally
            {
                db.Remove(testDocument);
            }
        }

        [Test]
        public void Remove_ThenQuery_ReturnsChanges()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.ReadYourOwnWrite)
            {
                Assert.Ignore("Cluster does not support RYOW, test skipped.");
            }

            var db = new BucketContext(bucket);
            var testValue = new Random().Next(0, 100000);
            var testKey = "Remove_ThenQuery_ReturnsChanges_" + testValue;

            var testDocument = new Sample
            {
                Id = testKey,
                Value = testValue
            };

            db.Save(testDocument);
            db.Remove(testDocument);

            Assert.NotNull(db.MutationState);

            var result = db.Query<Sample>()
                .ConsistentWith(db.MutationState)
                .Select(p => p.Id)
                .FirstOrDefault(p => p == testKey);

            Assert.Null(result);
        }

        [Test]
        public void SubmitChanges_ThenQuery_ReturnsChanges()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.ReadYourOwnWrite)
            {
                Assert.Ignore("Cluster does not support RYOW, test skipped.");
            }

            var db = new BucketContext(bucket);
            var testValue = new Random().Next(0, 100000);
            var testKey = "SubmitChanges_ThenQuery_ReturnsChanges_" + testValue;

            var testDocument = new Sample
            {
                Id = testKey,
                Value = testValue
            };

            db.BeginChangeTracking();
            db.Save(testDocument);
            db.SubmitChanges();

            try
            {
                Assert.NotNull(db.MutationState);

                var result = db.Query<Sample>()
                    .ConsistentWith(db.MutationState)
                    .FirstOrDefault(p => p.Id == testKey);

                Assert.NotNull(result);
                Assert.AreEqual(testValue, result.Value);
            }
            finally
            {
                db.Remove(testDocument);
            }
        }

        [Test]
        public void SubmitChanges_WithConsistencyCheck_Succeeds()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.ReadYourOwnWrite)
            {
                Assert.Ignore("Cluster does not support RYOW, test skipped.");
            }

            var db = new BucketContext(bucket);

            // Make doc to test
            db.Save(new BeerFiltered
            {
                Name = "TestBeer",
                BreweryId = "TestBrewery",
                Type = "beer",
                Abv = 1,
                Updated = DateTime.Now
            });

            db.BeginChangeTracking();

            var beer = db.Query<BeerFiltered>()
                .ConsistentWith(db.MutationState)
                .First(p => p.Name == "TestBeer");

            beer.Abv = 5;

            db.SubmitChanges();
        }

        [Test]
        public void SubmitChanges_WithConsistencyCheck_FailsOnCasMismatch()
        {
            var bucket = ClusterHelper.GetBucket("beer-sample");

            var clusterVersion = VersionProvider.Current.GetVersion(bucket);
            if (clusterVersion < FeatureVersions.ReadYourOwnWrite)
            {
                Assert.Ignore("Cluster does not support RYOW, test skipped.");
            }

            var db = new BucketContext(bucket);

            // Make doc to test
            db.Save(new BeerFiltered
            {
                Name = "TestBeer",
                BreweryId = "TestBrewery",
                Type = "beer",
                Abv = 1,
                Updated = DateTime.Now
            });

            db.BeginChangeTracking();

            var beer = db.Query<BeerFiltered>()
                .ConsistentWith(db.MutationState)
                .First(p => p.Name == "TestBeer" && p.BreweryId == "TestBrewery");

            // Alter document in separate context
            var db2 = new BucketContext(bucket);
            db2.Save(new BeerFiltered
            {
                Name = "TestBeer",
                BreweryId = "TestBrewery",
                Type = "beer",
                Abv = 2,
                Updated = DateTime.Now
            });

            // Alter document in tracked context
            beer.Abv = 5;

            Assert.Throws<CouchbaseConsistencyException>(() => db.SubmitChanges());
        }

        #endregion

        #region Helper Classes

        [DocumentTypeFilter("BucketContextTests_Sample")]
        public class Sample
        {
            [System.ComponentModel.DataAnnotations.Key]
            public string Id { get; set; }

            public string Type
            {
                get { return "BucketContextTests_Sample"; }
            }

            public int Value { get; set; }
        }

        #endregion
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
