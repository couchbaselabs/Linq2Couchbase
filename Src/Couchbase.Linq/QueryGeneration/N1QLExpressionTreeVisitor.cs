using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
        private readonly Dictionary<MethodInfo, Func<MethodCallExpression, Expression>> _methodCallTranslators = new Dictionary<MethodInfo, Func<MethodCallExpression, Expression>>();

        private Expression ContainsMethodTranslator(MethodCallExpression methodCallExpression)
        {  
                _expression.Append("(");
                VisitExpression(methodCallExpression.Object);
                _expression.Append(" LIKE '%");
                VisitExpression(methodCallExpression.Arguments[0]);
                _expression.Append("%')");
                return methodCallExpression;
       
        }

        private N1QlExpressionTreeVisitor(ParameterAggregator parameterAggregator)
        {
            _parameterAggregator = parameterAggregator; 
            _methodCallTranslators.Add(typeof(string).GetMethod("Contains"), ContainsMethodTranslator);
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

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            _expression.Append("(");

            VisitExpression(expression.Left);

            // In production code, handle this via lookup tables.
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    _expression.Append(" = ");
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    _expression.Append(" AND ");
                    break;

                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    _expression.Append(" OR ");
                    break;

                case ExpressionType.Add:
                    _expression.Append(" + ");
                    break;

                case ExpressionType.Subtract:
                    _expression.Append(" - ");
                    break;

                case ExpressionType.Multiply:
                    _expression.Append(" * ");
                    break;

                case ExpressionType.Divide:
                    _expression.Append(" / ");
                    break;
                case ExpressionType.GreaterThan:
                    _expression.Append(" > ");
                    break;

                case ExpressionType.LessThan:
                    _expression.Append(" < ");
                    break;
                case ExpressionType.NotEqual:
                    _expression.Append(" != "); //TODO: Change this to work for nulls. i.e. should be IS NOT NULL
                    break;           
                default:
                    base.VisitBinaryExpression(expression);
                    break;
            }

            VisitExpression(expression.Right);
            _expression.Append(")");

            return expression;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            return base.VisitSubQueryExpression(expression);
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            Func<MethodCallExpression, Expression> methodCallTranslator = null;

            if(_methodCallTranslators.TryGetValue(expression.Method, out methodCallTranslator))
            {
               return methodCallTranslator.Invoke(expression);
            }     
            else
            {
                return base.VisitMethodCallExpression(expression);
            }
        }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            var namedParameter = _parameterAggregator.AddNamedParameter(expression.Value);

            if (namedParameter.Value is string)
            {
                _expression.AppendFormat("'{0}'", namedParameter.Value);
            }
            else
            {
                _expression.AppendFormat("{0}", namedParameter.Value);
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
