using System;
using Couchbase.Linq.Clauses;
using Remotion.Linq;

namespace Couchbase.Linq.QueryGeneration
{
    internal interface IN1QlQueryModelVisitor : IQueryModelVisitor
    {
        void VisitNestClause(NestClause clause, QueryModel queryModel, int index);

        void VisitUseKeysClause(UseKeysClause clause, QueryModel queryModel, int index);

        void VisitHintClause(HintClause clause, QueryModel queryModel, int index);
    }
}
