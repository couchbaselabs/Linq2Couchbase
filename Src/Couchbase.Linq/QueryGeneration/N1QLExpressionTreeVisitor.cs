using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;

namespace Couchbase.Linq.QueryGeneration
{
    public class N1QlExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        private readonly StringBuilder _expression = new StringBuilder();
        private readonly ParameterAggregator _parameterAggregator;

        private N1QlExpressionTreeVisitor(ParameterAggregator parameterAggregator)
        {
            _parameterAggregator = parameterAggregator;
        }

        public static string GetN1QlExpression(Expression expression, ParameterAggregator aggregator)
        {
            var visitor = new N1QlExpressionTreeVisitor(aggregator);
            visitor.VisitExpression(expression);
            return visitor.GetN1QlExpression();
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            var expression = unhandledItem as Expression;
            var text = expression != null
                ? FormattingExpressionTreeVisitor.Format(expression)
                : unhandledItem.ToString();
           
            var message = string.Format(
                "The expression '{0}' (type: {1}) is not supported by this LINQ provider."
                , text
                , typeof (T));

            return new NotSupportedException(message);
        }

        public string GetN1QlExpression()
        {
            return _expression.ToString();
        }

        protected override Expression VisitNewExpression(NewExpression expression)
        {
            var members = expression.Members;
            for (var i = 0; i < members.Count; i++)
            {
                if (i == members.Count - 1)
                {
                    _expression.Append(members[i].Name);
                }
                else
                {   //add a delimiter to split on later
                    _expression.AppendFormat("{0},", members[i].Name);
                }
            }
            return expression;
        }

        protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
        {
            _expression.Append(expression.ReferencedQuerySource.ItemName);
            return expression;
        }

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            VisitExpression(expression.Expression);
            _expression.AppendFormat(".{0}", expression.Member.Name);
            return expression;
        }
    }
}
