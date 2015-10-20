using System;
using System.Linq;
using Couchbase.Linq.Filters;

namespace Couchbase.Linq.UnitTests.Documents
{
    class BreweryFilter : IDocumentFilter<Brewery>
    {
        public int Priority { get; set; }

        public IQueryable<Brewery> ApplyFilter(IQueryable<Brewery> source)
        {
            return source.Where(p => p.Type == "brewery");
        }
    }
}
