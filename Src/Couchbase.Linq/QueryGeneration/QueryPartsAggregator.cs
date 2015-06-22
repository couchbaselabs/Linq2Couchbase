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
            FromParts = new List<string>();
            WhereParts = new List<string>();
            OrderByParts = new List<string>();
        }

        public List<string> SelectParts { get; set; }
        public List<string> FromParts { get; set; }
        public List<string> WhereParts { get; set; }
        public List<string> OrderByParts { get; set; }
        public string LimitPart { get; set; }
        public string OffsetPart { get; set; }
        public string DistinctPart { get; set; }
        public string ExplainPart { get; set; }

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

        public void AddFromPart(string source)
        {
            FromParts.Add(source);
        }

        public void AddFromPart(IQuerySource source)
        {
            FromParts.Add(string.Format("{0} as {1}", source.ItemType.Name.ToLower(), source.ItemName));
        }

        public void AddDistinctPart(string value)
        {
            DistinctPart = value;
        }

        public string BuildN1QlQuery()
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

            if (FromParts.Any())
            {
                var mainFrom = FromParts.First();
                sb.AppendFormat(" FROM {0}", mainFrom.Replace(" as <generated>_1", "")); //TODO support multiple from parts
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
            var query = sb.ToString();
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