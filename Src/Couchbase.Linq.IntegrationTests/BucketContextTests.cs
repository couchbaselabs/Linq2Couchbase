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

            foreach (var beer in query.Take(1))
            {
                Console.WriteLine(beer.Name);
            }
        }

        [Test]
        public void BeerSampleContext_Tests()
        {
            var db = new BeerSample();
            var beers = from b in db.Beers
                select b;

            foreach (var beer in beers.Take(1))
            {
                Console.WriteLine(beer.Name);
            }
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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

            const string documentId = "21st_amendment_brewery_cafe-21a_ipa";

            var query = from x in db.Query<Beer>().UseKeys(new[] { documentId })
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.AreEqual(documentId, status.__metadata.Id);
        }

        [Test]
        public void Query_EnableProxyGeneration_ReturnsProxyBeerWithCas()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

            const string documentId = "21st_amendment_brewery_cafe-21a_ipa";

            var query = from x in db.Query<Beer>().UseKeys(new[] { documentId })
                        where x.Type == "beer"
                        select x;

            var beer = query.First();

            // ReSharper disable once SuspiciousTypeConversion.Global
            var status = beer as ITrackedDocumentNode;

            Assert.NotNull(status);
            Assert.Greater(status.__metadata.Cas, 0);
        }

        [Test]
        public void Query_EnableProxyGenerationChanges_FlagAsDirty()
        {
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
            var db = new BucketContext(ClusterHelper.GetBucket("beer-sample"))
            {
                EnableChangeTracking = true
            };

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
