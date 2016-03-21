using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Clauses;
using Remotion.Linq;

namespace Couchbase.Linq.QueryGeneration
{
    internal interface IN1QlQueryModelVisitor : IQueryModelVisitor
    {
        void VisitNestClause(NestClause clause, QueryModel queryModel, int index);

        void VisitUseKeysClause(UseKeysClause clause, QueryModel queryModel, int index);

        void VisitUpdateClause(UpdateClause clause, QueryModel queryModel, int index);
    }
}
