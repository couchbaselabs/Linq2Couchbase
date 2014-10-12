using Newtonsoft.Json;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Couchbase.Linq.QueryGeneration
{
    public class N1QlQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryPartsAggregator _queryPartsAggregator = new QueryPartsAggregator();
        private readonly ParameterAggregator _parameterAggregator = new ParameterAggregator();
        private readonly string _bucketName;

        public N1QlQueryModelVisitor(string bucketName)
        {
            _bucketName = bucketName;
        }

        public static string GenerateN1QlQuery(QueryModel queryModel, string bucketName)
        {
            var visitor = new N1QlQueryModelVisitor(bucketName);
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }

        public string GetQuery()
        {
            return _queryPartsAggregator.BuildN1QlQuery();
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            //In N1QL, the source is the bucket, not the document
            _queryPartsAggregator.AddFromPart(string.Format("{0} as {1}", _bucketName, fromClause.ItemName)); 
            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            foreach (var parameter in GetSelectParameters(selectClause, queryModel))
            {
                _queryPartsAggregator.AddSelectParts(parameter); 
            }
            base.VisitSelectClause(selectClause, queryModel);
        }

        private IEnumerable<string> GetSelectParameters(SelectClause selectClause, QueryModel queryModel)
        {
            var expressions = new List<string>();
            var prefix = queryModel.MainFromClause.ItemName;
            var expression = GetN1QlExpression(selectClause.Selector);
            if (expression.Contains(','))
            {
                expressions.
                    AddRange(expression.Split(',').
                    Select(value => string.Format("{0}.{1}", prefix, value)));
            }
            else
            {
                var selectType = selectClause.Selector.GetType();
                var property = TypeDescriptor.GetProperties(selectType)["Member"];
                if (property != null)
                {
                    var fieldName = (PropertyInfo)property.GetValue(selectClause.Selector);
                    var attribute = fieldName.GetCustomAttributes().FirstOrDefault(x => x is JsonPropertyAttribute);
                    if (attribute != null)
                    {
                        var temp = (JsonPropertyAttribute)attribute;
                        expression = temp.PropertyName;

                        if (!string.IsNullOrEmpty(prefix))
                        {
                            expression = string.Format("{0}.{1}", prefix, expression);
                        }
                    }
                }
                expressions.Add(expression);
            }
            return expressions;
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            _queryPartsAggregator.AddWherePart(GetN1QlExpression(whereClause.Predicate));
            base.VisitWhereClause(whereClause, queryModel, index);
        }


        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            var orderByParts = orderByClause.Orderings.Select(ordering => String.Concat(GetN1QlExpression(ordering.Expression), " ", ordering.OrderingDirection.ToString().ToUpper())).ToList();

            _queryPartsAggregator.AddOrderByPart(orderByParts);

            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        private string GetN1QlExpression(Expression expression)
        {
            return N1QlExpressionTreeVisitor.GetN1QlExpression(expression, _parameterAggregator);
        }
    }
}
