using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration
{
    class UnclaimedGroupJoin
    {

        public JoinClause JoinClause { get; set; }
        public GroupJoinClause GroupJoinClause { get; set; }

    }
}
