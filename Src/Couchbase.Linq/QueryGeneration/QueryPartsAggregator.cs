using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration
{
    internal class QueryPartsAggregator
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public QueryPartsAggregator()
        {
            FromParts = new List<N1QlFromQueryPart>();
            LetParts = new List<N1QlLetQueryPart>();
            WhereParts = new List<string>();
            OrderByParts = new List<string>();
        }

        public string SelectPart { get; set; }
        public List<N1QlFromQueryPart> FromParts { get; set; }
        public string UseKeysPart { get; set; }
        public List<N1QlLetQueryPart> LetParts { get; set; } 
        public List<string> WhereParts { get; set; }
        public List<string> OrderByParts { get; set; }
        public List<string> GroupByParts { get; set; } 
        public List<string> HavingParts { get; set; } 
        public string LimitPart { get; set; }
        public string OffsetPart { get; set; }
        public string DistinctPart { get; set; }
        public string ExplainPart { get; set; }
        public string MetaPart { get; set; }
        public string WhereAllPart { get; set; }
        /// <summary>
        /// For subqueries, stores the name of property to extract to a plain array
        /// </summary>
        public string PropertyExtractionPart { get; set; }
        /// <summary>
        /// For Array subqueries, list of functions to wrap the result
        /// </summary>
        public List<string> WrappingFunctions { get; set; }
        /// <summary>
        /// For aggregates, wraps the SelectPart with this function call
        /// </summary>
        public string AggregateFunction { get; set; }

        /// <summary>
        /// Indicates the type of query or subquery being generated
        /// </summary>
        /// <remarks>
        /// Defaults to building a SELECT query
        /// </remarks>
        public N1QlQueryType QueryType { get; set; }

        /// <summary>
        /// Returns true if the QueryType is a bucket-based subquery
        /// </summary>
        public bool IsBucketSubquery
        {
            get
            {
                return (QueryType == N1QlQueryType.Subquery) || (QueryType == N1QlQueryType.SubqueryAny) ||
                       (QueryType == N1QlQueryType.SubqueryAll);
            }
        }

        /// <summary>
        /// Returns true if the QueryType is an array-based subquery
        /// </summary>
        public bool IsArraySubquery
        {
            get
            {
                return (QueryType == N1QlQueryType.Array) || (QueryType == N1QlQueryType.ArrayAny) ||
                       (QueryType == N1QlQueryType.ArrayAll);
            }
        }

        public void AddWhereMissingPart(string format, params object[] args)
        {
            WhereParts.Add(string.Format(format, args));
        }

        public void AddWherePart(string format, params object[] args)
        {
            WhereParts.Add(string.Format(format, args));
        }

        public void AddFromPart(N1QlFromQueryPart fromPart)
        {
            FromParts.Add(fromPart);
        }

        public void AddUseKeysPart(string useKeysPart)
        {
            if (!string.IsNullOrEmpty(UseKeysPart))
            {
                throw new InvalidOperationException("AddUseKeysPart May Only Be Called Once");
            }

            UseKeysPart = useKeysPart;
        }

        public void AddLetPart(N1QlLetQueryPart letPart)
        {
            LetParts.Add(letPart);
        }

        public void AddDistinctPart(string value)
        {
            DistinctPart = value;
        }

        /// <summary>
        /// Adds an expression to the comma-delimited list of the GROUP BY clause
        /// </summary>
        public void AddGroupByPart(string value)
        {
            if (GroupByParts == null)
            {
                GroupByParts = new List<string>();
            }

            GroupByParts.Add(value);
        }

        /// <summary>
        /// Adds an expression to the HAVING clause, ANDed with any other expressions
        /// </summary>
        public void AddHavingPart(string value)
        {
            if (HavingParts == null)
            {
                HavingParts = new List<string>();
            }

            HavingParts.Add(value);
        }

        private void ApplyLetParts(StringBuilder sb)
        {
            for (var i = 0; i < LetParts.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(" LET ");
                }
                else
                {
                    sb.Append(", ");
                }

                sb.AppendFormat("{0} = {1}", LetParts[i].ItemName, LetParts[i].Value);
            }
        }

        public void AddWrappingFunction(string function)
        {
            if (WrappingFunctions == null)
            {
                WrappingFunctions = new List<string>();
            }

            WrappingFunctions.Add(function);
        }

        /// <summary>
        /// Builds a primary select query
        /// </summary>
        /// <returns>Query string</returns>
        private string BuildSelectQuery()
        {
            var sb = new StringBuilder();
            
            if (QueryType == N1QlQueryType.Subquery)
            {
                if (!string.IsNullOrEmpty(PropertyExtractionPart))
                {
                    // Subqueries will always return a list of objects
                    // But we need to use an ARRAY statement to convert it into an array of a particular property of that object

                    sb.AppendFormat("ARRAY `ArrayExtent`.{0} FOR `ArrayExtent` IN (", PropertyExtractionPart);
                }
                else
                {
                    sb.Append('(');
                }
            }
            else if (QueryType == N1QlQueryType.SubqueryAny)
            {
                sb.AppendFormat("ANY {0} IN (", PropertyExtractionPart);
            }
            else if (QueryType == N1QlQueryType.SubqueryAll)
            {
                sb.AppendFormat("EVERY {0} IN (", PropertyExtractionPart);
            }

            if (!string.IsNullOrWhiteSpace(ExplainPart))
            {
                sb.Append(ExplainPart);
            }

            if (!string.IsNullOrEmpty(AggregateFunction))
            {
                sb.AppendFormat("SELECT {0}({1}{2})",
                    AggregateFunction,
                    !string.IsNullOrWhiteSpace(DistinctPart) ? DistinctPart : string.Empty,
                    SelectPart);
            }
            else
            {
                sb.AppendFormat("SELECT {0}{1}",
                    !string.IsNullOrWhiteSpace(DistinctPart) ? DistinctPart : string.Empty,
                    SelectPart);
                //TODO support multiple select parts: http://localhost:8093/tutorial/content/#5
            }

            if (!IsBucketSubquery && !string.IsNullOrEmpty(PropertyExtractionPart))
            {
                sb.AppendFormat(" as {0}", PropertyExtractionPart);
            }

            if (FromParts.Any())
            {
                var mainFrom = FromParts.First();
                sb.AppendFormat(" FROM {0} as {1}",
                    mainFrom.Source,
                    mainFrom.ItemName);

                if (!string.IsNullOrEmpty(UseKeysPart))
                {
                    sb.AppendFormat(" USE KEYS {0}", UseKeysPart);
                }

                foreach (var joinPart in FromParts.Skip(1))
                {
                    sb.AppendFormat(" {0} {1} as {2}",
                        joinPart.JoinType,
                        joinPart.Source,
                        joinPart.ItemName);

                    if (!string.IsNullOrEmpty(joinPart.OnKeys))
                    {
                        sb.AppendFormat(" ON KEYS {0}", joinPart.OnKeys);
                    }
               }
            }

            ApplyLetParts(sb);

            if (WhereParts.Any())
            {
                sb.AppendFormat(" WHERE {0}", String.Join(" AND ", WhereParts));
            }
            if ((GroupByParts != null) && GroupByParts.Any())
            {
                sb.AppendFormat(" GROUP BY {0}", string.Join(", ", GroupByParts));
            }
            if ((HavingParts != null) && HavingParts.Any())
            {
                sb.AppendFormat(" HAVING {0}", string.Join(" AND ", HavingParts));
            }
            if (OrderByParts.Any())
            {
                sb.AppendFormat(" ORDER BY {0}", String.Join(", ", OrderByParts));
            }
            if (LimitPart != null)
            {
                sb.Append(LimitPart);
            }
            if (LimitPart != null && OffsetPart != null)
            {
                sb.Append(OffsetPart);
            }

            if (QueryType == N1QlQueryType.Subquery)
            {
                if (!string.IsNullOrEmpty(PropertyExtractionPart))
                {
                    sb.Append(") END");
                }
                else
                {
                    sb.Append(')');
                }
            }
            else if (QueryType == N1QlQueryType.SubqueryAny)
            {
                sb.Append(") SATISFIES true END");
            }
            else if (QueryType == N1QlQueryType.SubqueryAll)
            {
                sb.AppendFormat(") SATISFIES {0} END", WhereAllPart);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a main query to return an Any or All result
        /// </summary>
        /// <returns>Query string</returns>
        private string BuildMainAnyAllQuery()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("SELECT {0} as result",
                QueryType == N1QlQueryType.MainQueryAny ? "true" : "false");

            if (FromParts.Any())
            {
                var mainFrom = FromParts.First();
                sb.AppendFormat(" FROM {0} as {1}",
                    mainFrom.Source,
                    mainFrom.ItemName);

                if (!string.IsNullOrEmpty(UseKeysPart))
                {
                    sb.AppendFormat(" USE KEYS {0}", UseKeysPart);
                }

                foreach (var joinPart in FromParts.Skip(1))
                {
                    sb.AppendFormat(" {0} {1} as {2}",
                        joinPart.JoinType,
                        joinPart.Source,
                        joinPart.ItemName);

                    if (!string.IsNullOrEmpty(joinPart.OnKeys))
                    {
                        sb.AppendFormat(" ON KEYS {0}", joinPart.OnKeys);
                    }
                }
            }

            ApplyLetParts(sb);

            bool hasWhereClause = false;
            if (WhereParts.Any())
            {
                sb.AppendFormat(" WHERE {0}", String.Join(" AND ", WhereParts));

                hasWhereClause = true;
            }

            if (QueryType == N1QlQueryType.MainQueryAll)
            {
                sb.AppendFormat(" {0} NOT ({1})",
                    hasWhereClause ? "AND" : "WHERE",
                    WhereAllPart);
            }

            sb.Append(" LIMIT 1");

            return sb.ToString();
        }

        /// <summary>
        /// Build a subquery against a nested array property
        /// </summary>
        /// <returns></returns>
        private string BuildArrayQuery()
        {
            var sb = new StringBuilder();

            var mainFrom = FromParts.FirstOrDefault();
            if (mainFrom == null)
            {
                throw new InvalidOperationException("N1QL Subquery Missing From Part");
            }

            if (WrappingFunctions != null)
            {
                foreach (string function in WrappingFunctions.AsEnumerable().Reverse())
                {
                    sb.AppendFormat("{0}(", function);
                }
            }

            if ((SelectPart != mainFrom.ItemName) || WhereParts.Any())
            {
                sb.AppendFormat("ARRAY {0} FOR {1} IN {2}", SelectPart, mainFrom.ItemName, mainFrom.Source);

                if (WhereParts.Any())
                {
                    sb.AppendFormat(" WHEN {0}", String.Join(" AND ", WhereParts));
                }

                sb.Append(" END");
            }
            else
            {
                // has no projection or predicates, so simplify

                sb.Append(mainFrom.Source);
            }

            if (WrappingFunctions != null)
            {
                sb.Append(')', WrappingFunctions.Count);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a subquery using the ANY or EVERY expression to test a nested array
        /// </summary>
        /// <returns>Query string</returns>
        private string BuildAnyAllQuery()
        {
            var sb = new StringBuilder();

            var mainFrom = FromParts.FirstOrDefault();
            if (mainFrom == null)
            {
                throw new InvalidOperationException("N1QL Any Subquery Missing From Part");
            }

            var source = mainFrom.Source;
            if ((QueryType == N1QlQueryType.ArrayAll) && WhereParts.Any())
            {
                // WhereParts should be used to filter the source before the EVERY query
                // This is done using the ARRAY operator with a WHEN clause

                source = String.Format("(ARRAY {1} FOR {1} IN {0} WHEN {2} END)",
                    source,
                    mainFrom.ItemName,
                    String.Join(" AND ", WhereParts));
            }

            sb.AppendFormat("{0} {1} IN {2} ",
                QueryType == N1QlQueryType.ArrayAny ? "ANY" : "EVERY",
                mainFrom.ItemName,
                source);

            if (QueryType == N1QlQueryType.ArrayAny)
            {
                // WhereParts should be applied to the SATISFIES portion of the query

                if (WhereParts.Any())
                {
                    sb.AppendFormat("SATISFIES {0}", String.Join(" AND ", WhereParts));
                }
                else
                {
                    sb.Append("SATISFIES true");
                }
            }
            else // N1QlQueryType.All
            {
                // WhereAllPart is applied as the SATISFIES portion of the query

                sb.Append("SATISFIES ");
                sb.Append(WhereAllPart);
            }

            sb.Append(" END");

            return sb.ToString();
        }

        private string BuildAggregate()
        {
            return string.Format("{0}({1})", AggregateFunction, SelectPart);
        }

        public string BuildN1QlQuery()
        {
            string query;

            switch (QueryType)
            {
                case N1QlQueryType.Select:
                case N1QlQueryType.Subquery:
                case N1QlQueryType.SubqueryAny:
                case N1QlQueryType.SubqueryAll:
                    query = BuildSelectQuery();
                    break;

                case N1QlQueryType.Array:
                    query = BuildArrayQuery();
                    break;

                case N1QlQueryType.ArrayAny:
                case N1QlQueryType.ArrayAll:
                    query = BuildAnyAllQuery();
                    break;

                case N1QlQueryType.MainQueryAny:
                case N1QlQueryType.MainQueryAll:
                    query = BuildMainAnyAllQuery();
                    break;

                case N1QlQueryType.Aggregate:
                    query = BuildAggregate();
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Unsupported N1QlQueryType: {0}", QueryType));
            }

            Log.Debug(query);
            return query;
        }

        public void AddOffsetPart(string offsetPart, int count)
        {
            OffsetPart = String.Format(offsetPart, count);
        }

        public void AddLimitPart(string limitPart, int count)
        {
            LimitPart = String.Format(limitPart, count);
        }

        public void AddOrderByPart(IEnumerable<string> orderings)
        {
            OrderByParts.Insert(0, String.Join(", ", orderings));
        }
    }
}