using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;

namespace Couchbase.Linq.QueryGeneration
{
    public class N1QlExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        private readonly StringBuilder _expression = new StringBuilder();

        private readonly Dictionary<MethodInfo, Func<MethodCallExpression, Expression>> _methodCallTranslators =
            new Dictionary<MethodInfo, Func<MethodCallExpression, Expression>>();

        private readonly IMemberNameResolver _nameResolver = new JsonNetMemberNameResolver();
        private readonly ParameterAggregator _parameterAggregator;

        private N1QlExpressionTreeVisitor(ParameterAggregator parameterAggregator)
        {
            _parameterAggregator = parameterAggregator;
            _methodCallTranslators.Add(typeof (string).GetMethod("Contains"), ContainsMethodTranslator);
            _methodCallTranslators.Add(typeof (N1Ql).GetMethod("Meta"), MetaMethodTranslator);
        }

        #region Method Translators

        private Expression ContainsMethodTranslator(MethodCallExpression methodCallExpression)
        {
            _expression.Append("(");
            VisitExpression(methodCallExpression.Object);
            _expression.Append(" LIKE '%");

            var indexInsertStarted = _expression.Length;

            VisitExpression(methodCallExpression.Arguments[0]);

            var indexInsertEnded = _expression.Length;

            _expression.Append("%')");

            //Remove extra quote marks which have been added due to the string in the clause, these aren't needed as they have been added already in this case.
            _expression.Remove(indexInsertStarted, 1);
            _expression.Remove(indexInsertEnded - 2, 1);

            return methodCallExpression;
        }

        private Expression MetaMethodTranslator(MethodCallExpression methodCallExpression)
        {
            _expression.Append("META(");
            VisitExpression(methodCallExpression.Arguments[0]);
            _expression.Append(')');

            return methodCallExpression;
        }

        #endregion

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
            var arguments = expression.Arguments;
            var members = expression.Members;

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                {
                    _expression.Append(",");
                }

                var expressionLength = _expression.Length;

                VisitExpression(arguments[i]);

                //only add 'as' part if the  previous visitexpression has generated something.
                if (_expression.Length > expressionLength)
                {
                    _expression.AppendFormat(" as {0}", N1QlQueryModelVisitor.EscapeIdentifier(members[i].Name));
                }
            }

            return expression;
        }

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            _expression.Append("(");

            VisitExpression(expression.Left);

            //TODO: Refactor this to work in a nicer way. Maybe use lookup tables
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

        /// <summary>
        ///     Tries to translate the Method-call to some N1QL expression. Currently only implemented for "Contains() - LIKE"
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            Func<MethodCallExpression, Expression> methodCallTranslator = null;

            if (_methodCallTranslators.TryGetValue(expression.Method, out methodCallTranslator))
            {
                return methodCallTranslator.Invoke(expression);
            }
            return base.VisitMethodCallExpression(expression);
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
            _expression.Append(N1QlQueryModelVisitor.EscapeIdentifier(expression.ReferencedQuerySource.ItemName));
            return expression;
        }

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            string memberName;

            if (_nameResolver.TryResolveMemberName(expression.Member, out memberName))
            {
                VisitExpression(expression.Expression);
                _expression.AppendFormat(".{0}", N1QlQueryModelVisitor.EscapeIdentifier(memberName));
            }

            return expression;
        }

        protected override Expression VisitUnaryExpression(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    _expression.Append("NOT ");
                    VisitExpression(expression.Operand);
                    break;

                default:
                    VisitExpression(expression.Operand);
                    break;
            }
            
            return expression;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            var modelVisitor = new N1QlQueryModelVisitor();

            modelVisitor.VisitQueryModel(expression.QueryModel);
            _expression.Append(modelVisitor.GetQuery());

            return expression;
        }
    }
}