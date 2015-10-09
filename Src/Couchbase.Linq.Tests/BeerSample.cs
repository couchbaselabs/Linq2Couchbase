using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Filters;

using Couchbase.Linq.Tests.Documents;

namespace Couchbase.Linq.Tests
{
    /// <summary>
    /// A concrete DbContext for the beer-sample example bucket.
    /// </summary>
    public class BeerSample : BucketContext
    {
        public BeerSample()
            : this(ClusterHelper.GetBucket("beer-sample"))
        {
        }

        public BeerSample(IBucket bucket) : base(bucket)
        {
            //Two ways of applying a filter are included in this example.
            //This is by implementing IDocumentFilter and then adding explicitly.
            //adding it to the DocumentFilterManager
            DocumentFilterManager.SetFilter(new BreweryFilter());
        }

        public IQueryable<BeerFiltered> Beers
        {
            //This is an example of adding a filter declaratively by using an atribute
            //to your document. If you check out BeerFiltered clas you will see the DocumentTypeFilter
            //has been added to the class definition.
            get { return Query<BeerFiltered>(); }
        }

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
