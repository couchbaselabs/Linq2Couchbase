using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration.ExpressionTransformers
{
    /// <summary>
    /// Default registry of expression transformers used to convert DateTime expressions as needed for N1QL
    /// </summary>
    class DateTimeTransformationRegistry
    {
        /// <summary>
        /// Default registry of expression transformers used to convert DateTime expressions as needed for N1QL
        /// </summary>
        public static ExpressionTransformerRegistry Default { get; set; }

        static DateTimeTransformationRegistry()
        {
            var registry = new ExpressionTransformerRegistry();
            registry.Register(new DateTimeComparisonExpressionTransformer());

            Default = registry;
        }
    }
}
