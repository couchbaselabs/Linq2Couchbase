using System;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Couchbase.Linq.Utils
{
    /// <summary>
    /// No-op implementation of <see cref="IEvaluatableExpressionFilter"/>, which treats
    /// all expressions as evaluatable.
    /// </summary>
    internal sealed class NullEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
    }
}
