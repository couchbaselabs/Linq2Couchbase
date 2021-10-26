using System.Linq;
using Couchbase.KeyValue;
using Couchbase.Linq.Filters;

using Couchbase.Linq.IntegrationTests.Documents;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq.IntegrationTests
{
    /// <summary>
    /// A concrete DbContext for the beer-sample example bucket.
    /// </summary>
    public class BeerSample : BucketContext
    {
        public BeerSample()
            : this(TestSetup.Bucket)
        {
        }

        public BeerSample(IBucket bucket) : base(bucket)
        {
            //Two ways of applying a filter are included in this example.
            //This is by implementing IDocumentFilter and then adding explicitly.
            //adding it to the DocumentFilterManager

            bucket.Cluster.ClusterServices.GetRequiredService<DocumentFilterManager>().SetFilter(new BreweryFilter());
        }

        public IDocumentSet<BeerFiltered> Beers { get; set; } = null!;

        public IQueryable<Brewery> Breweries
        {
            get { return Query<Brewery>(); }
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
