using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;

namespace Couchbase.Linq.UnitTests
{
    /// <summary>
    /// Used to test query generation for result operators that always execute the query immediately.
    /// This class fakes the result (always returns null or an empty list), but stores the query that was 
    /// generated in the Query property.
    /// </summary>
    public class BucketQueryExecutorEmulator : IQueryExecutor
    {
        private readonly N1QLTestBase _test;
        public N1QLTestBase Test
        {
            get { return _test; }
        }

        private string _query;
        public string Query
        {
            get { return _query; }
        }

        public BucketQueryExecutorEmulator(N1QLTestBase test)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            _test = test;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            _query = ExecuteCollection(queryModel);

            return new T[] {};
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            ExecuteCollection<T>(queryModel);
            return default(T);
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            ExecuteCollection<T>(queryModel);
            return default(T);
        }

        public string ExecuteCollection(QueryModel queryModel)
        {
            var queryGenerationContext = new N1QlQueryGenerationContext()
            {
                MemberNameResolver = new JsonNetMemberNameResolver(Test.ContractResolver),
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider(),
                Serializer = new Core.Serialization.DefaultSerializer()
            };

            var visitor = new N1QlQueryModelVisitor(queryGenerationContext);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }
    }
}