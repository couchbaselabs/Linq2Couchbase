using System;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    internal static class JoinTypes
    {
        public const string InnerJoin = "INNER JOIN";
        public const string LeftJoin = "LEFT JOIN";
        public const string InnerNest = "INNER NEST";
        public const string LeftNest = "LEFT OUTER NEST";
        public const string InnerUnnest = "INNER UNNEST";
        public const string LeftUnnest = "LEFT OUTER UNNEST";
    }
}
