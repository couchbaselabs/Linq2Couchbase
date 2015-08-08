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
    public interface IN1QlQueryModelVisitor : IQueryModelVisitor
    {
        void VisitNestClause(NestClause clause, QueryModel queryModel, int index);

        void VisitUseKeysClause(UseKeysClause clause, QueryModel queryModel, int index);

        void VisitWhereMissingClause(WhereMissingClause clause, QueryModel queryModel, int index);
    }
}
