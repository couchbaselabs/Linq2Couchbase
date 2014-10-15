using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq.Tests
{
// ReSharper disable once InconsistentNaming
    public class N1QLTestBase
    {
        protected string CreateN1QlQuery(IBucket bucket, Expression expression)
        {
            var queryModel = QueryParserHelper.CreateQueryParser().GetParsedQuery(expression);
            return N1QlQueryModelVisitor.GenerateN1QlQuery(queryModel, bucket.Name);
        }

    }
}
