using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Version;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Filters;
using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Utils;
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
            var db = new BucketContext(TestSetup.Bucket);
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

        [Test]
        public async Task InheritedContext_Basic()
        {
            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion.Version < new Version(7, 0, 0))
            {
                Assert.Ignore("Skipping due to lack of collection support.");
            }

            var db = new TravelSample(await TestSetup.Cluster.BucketAsync("travel-sample"));
            var query = from route in db.Routes
                select route;

            foreach (var route in query.Take(1))
            {
                Console.WriteLine(route.Airline);
            }
        }

        [Test]
        public async Task InheritedContext_CanQueryMultipleTimes()
        {
            var versionProvider = TestSetup.Cluster.ClusterServices.GetRequiredService<IClusterVersionProvider>();
            var clusterVersion = await versionProvider.GetVersionAsync() ?? FeatureVersions.DefaultVersion;
            if (clusterVersion.Version < new Version(7, 0, 0))
            {
                Assert.Ignore("Skipping due to lack of collection support.");
            }

            var db = new TravelSample(await TestSetup.Cluster.BucketAsync("travel-sample"));
            var query = from route in db.Routes
                select route;

            foreach (var route in query.Take(1))
            {
                Console.WriteLine(route.Airline);
            }

            query = from route in db.Routes
                select route;

            foreach (var route in query.Skip(1).Take(1))
            {
                Console.WriteLine(route.Airline);
            }
        }

        [Test]
        public void InheritedContext_AppliesDocumentFilters()
        {
            var db = new BeerSample();
            var query = from beer in db.Beers
                select beer;

            foreach (var beer in query.Skip(5).Take(1))
            {
                Assert.Greater(beer.Abv, 0);
                Console.WriteLine(beer.Abv);
            }
        }

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
