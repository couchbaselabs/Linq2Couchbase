using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration
{
    public class QueryPartsAggregator
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public QueryPartsAggregator()
        {
            SelectParts = new List<string>();
            FromParts = new List<N1QlFromQueryPart>();
            WhereParts = new List<string>();
            OrderByParts = new List<string>();
        }

        public List<string> SelectParts { get; set; }
        public List<N1QlFromQueryPart> FromParts { get; set; }
        public List<string> WhereParts { get; set; }
        public List<string> OrderByParts { get; set; }
        public string LimitPart { get; set; }
        public string OffsetPart { get; set; }
        public string DistinctPart { get; set; }
        public string ExplainPart { get; set; }
        public string MetaPart { get; set; }
        public string WhereAllPart { get; set; }

        /// <summary>
        /// Indicates the type of query or subquery being generated
        /// </summary>
        /// <remarks>
        /// Defaults to building a SELECT query
        /// </remarks>
        public N1QlQueryType QueryType { get; set; }

        public void AddSelectParts(string format, params object[] args)
        {
            SelectParts.Add(string.Format(format, args));
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

        public void AddDistinctPart(string value)
        {
            DistinctPart = value;
        }

        /// <summary>
        /// Builds a primary select query
        /// </summary>
        /// <returns>Query string</returns>
        private string BuildSelectQuery()
        {
            var sb = new StringBuilder();
            var selectParts = new StringBuilder();
            for (var i = 0; i < SelectParts.Count; i++)
            {
                if (i == SelectParts.Count - 1)
                {
                    selectParts.Append(SelectParts[i]);
                }
                else
                {
                    selectParts.AppendFormat("{0}, ", SelectParts[i]);
                }
            }

            if (!string.IsNullOrWhiteSpace(ExplainPart))
            {
                sb.Append(ExplainPart);
            }
            sb.AppendFormat("SELECT {0}{1}", string.IsNullOrWhiteSpace(DistinctPart) ? string.Empty : DistinctPart,  selectParts);
                //TODO support multiple select parts: http://localhost:8093/tutorial/content/#5

            if (!string.IsNullOrWhiteSpace(MetaPart))
            {
                sb.AppendFormat(SelectParts.Count > 0 ? ", {0}" : "{0}", MetaPart);
            }
            if (FromParts.Any())
            {
                var mainFrom = FromParts.First();
                sb.AppendFormat(" FROM {0} as {1}",
                    mainFrom.Source,
                    mainFrom.ItemName); //TODO support multiple from parts
            }
            if (WhereParts.Any())
            {
                sb.AppendFormat(" WHERE {0}", String.Join(" AND ", WhereParts));
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

            return sb.ToString();
        }

        /// <summary>
        /// Builds a subquery using the ANY expression to test a nested array
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
            if ((QueryType == N1QlQueryType.All) && WhereParts.Any())
            {
                // WhereParts should be used to filter the source before the EVERY query
                // This is done using the ARRAY operator with a WHEN clause

                source = String.Format("(ARRAY {1} FOR {1} IN {0} WHEN {2} END)",
                    source,
                    mainFrom.ItemName,
                    String.Join(" AND ", WhereParts));
            }

            sb.AppendFormat("{0} {1} IN {2} ",
                QueryType == N1QlQueryType.Any ? "ANY" : "EVERY",
                mainFrom.ItemName,
                source);

            if (QueryType == N1QlQueryType.Any)
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

        public string BuildN1QlQuery()
        {
            string query;

            switch (QueryType)
            {
                case N1QlQueryType.Select:
                    query = BuildSelectQuery();
                    break;

                case N1QlQueryType.Any:
                case N1QlQueryType.All:
                    query = BuildAnyAllQuery();
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