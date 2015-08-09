using System;
using System.Linq;
using Couchbase.Linq.Tests.Documents;
using NUnit.Framework;

namespace Couchbase.Linq.Tests
{
    [TestFixture]
    public class DbContextTests
    {
        [Test]
        public void Test_Basic_Query()
        {
            ClusterHelper.Initialize();
            var cluster = ClusterHelper.Get();

            var db = new DbContext(cluster, "beer-sample");
            var query = from x in db.Query<Beer>()
                select x;

            foreach (var beer in query)
            {
                Console.WriteLine(beer.Name);
            }
        }

        [Test]
        public void BeerSampleContext_Tests()
        {
            ClusterHelper.Initialize();

            var db = new BeerSample();
            var beers = from b in db.Beers
                select b;

            foreach (var beer in beers)
            {
                Console.WriteLine(beer.Name);
            }
        }

        [Test]
        public void BeerSample_Tests()
        {
            ClusterHelper.Initialize();

            var db = new BeerSample();
            var query = from beer in db.Beers
                        join brewery in db.Breweries
                        on beer.BreweryId equals N1Ql.Key(brewery)
                        select new { beer.Name, beer.Abv, BreweryName = brewery.Name };

            foreach (var beer in query)
            {
                Console.WriteLine(beer.Name);
            }

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
