using System;
using System.Collections.Generic;
using System.Linq;
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
        public StringBuilder Expression
        {
            get { return _expression; }
        }

        private readonly IMemberNameResolver _nameResolver = new JsonNetMemberNameResolver();
        private readonly ParameterAggregator _parameterAggregator;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        private N1QlExpressionTreeVisitor(ParameterAggregator parameterAggregator, IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _parameterAggregator = parameterAggregator;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public static string GetN1QlExpression(Expression expression, ParameterAggregator aggregator, IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            var visitor = new N1QlExpressionTreeVisitor(aggregator, methodCallTranslatorProvider);
            visitor.VisitExpression(expression);
            return visitor.GetN1QlExpression();
        }

        public static string GetN1QlSelectNewExpression(NewExpression expression, ParameterAggregator aggregator, IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            var visitor = new N1QlExpressionTreeVisitor(aggregator, methodCallTranslatorProvider);
            visitor.VisitSelectNewExpression(expression);
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

            _expression.Append('{');

            for (var i = 0; i < members.Count; i++)
            {
                var beforeAppendLength = _expression.Length;

                if (i > 0)
                {
                    _expression.Append(", ");
                }

                _expression.AppendFormat("\"{0}\": ", members[i].Name);

                var beforeSubExpressionLength = _expression.Length;

                VisitExpression(arguments[i]);

                if (_expression.Length == beforeSubExpressionLength)
                {                    
                    // nothing was added for the value, so remove the part that was added originally
                    _expression.Length = beforeAppendLength;
                }
            }

            _expression.Append('}');

            return expression;
        }

        /// <summary>
        /// Parses the new object that is part of the select expression with "as" based formatting
        /// </summary>
        private Expression VisitSelectNewExpression(NewExpression expression)
        {
            var arguments = expression.Arguments;
            var members = expression.Members;

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                {
                    _expression.Append(", ");
                }

                var expressionLength = _expression.Length;

                VisitExpression(arguments[i]);

                //only add 'as' part if the  previous visitexpression has generated something.
                if (_expression.Length > expressionLength)
                {
                    _expression.AppendFormat(" as {0}", N1QlQueryModelVisitor.EscapeIdentifier(members[i].Name));
                }
                else if (i > 0)
                {
                    // nothing was added, so remove the extra comma
                    _expression.Length -= 2;
                }
            }

            return expression;
        }

        protected override Expression VisitNewArrayExpression(NewArrayExpression expression)
        {
            if (expression.NodeType == ExpressionType.NewArrayInit)
            {
                _expression.Append('[');

                for (var i=0; i<expression.Expressions.Count; i++)
                {
                    if (i > 0)
                    {
                        _expression.Append(", ");
                    }

                    VisitExpression(expression.Expressions[i]);
                }

                _expression.Append(']');

                return expression;
            }
            else
            {
                return base.VisitNewArrayExpression(expression);
            }
        }

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {   
            ConstantExpression constantExpression;

            _expression.Append("(");

            VisitExpression(expression.Left);

            //TODO: Refactor this to work in a nicer way. Maybe use lookup tables
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    constantExpression = expression.Right as ConstantExpression;
                    if ((constantExpression != null) && (constantExpression.Value == null))
                    {
                        // short circuit normal path to use IS NULL operator
                        _expression.Append(" IS NULL)");
                        return expression;
                    }
                    else
                    {
                        _expression.Append(" = ");
                    }
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
                    if ((expression.Left.Type != typeof (string)) || (expression.Right.Type != typeof (string)))
                    {
                        _expression.Append(" + ");
                    }
                    else
                    {
                        _expression.Append(" || ");
                    }
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

                case ExpressionType.Modulo:
                    _expression.Append(" % ");
                    break;

                case ExpressionType.GreaterThan:
                    _expression.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _expression.Append(" >= ");
                    break;

                case ExpressionType.LessThan:
                    _expression.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _expression.Append(" <= ");
                    break;

                case ExpressionType.NotEqual:
                    constantExpression = expression.Right as ConstantExpression;
                    if ((constantExpression != null) && (constantExpression.Value == null))
                    {
                        // short circuit normal path to use IS NOT NULL operator
                        _expression.Append(" IS NOT NULL)");
                        return expression;
                    }
                    else
                    {
                        _expression.Append(" != ");
                    }
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
            IMethodCallTranslator methodCallTranslator = _methodCallTranslatorProvider.GetTranslator(expression);

            if (methodCallTranslator != null)
            {
                return methodCallTranslator.Translate(expression, this);
            }

            return base.VisitMethodCallExpression(expression);
        }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            var namedParameter = _parameterAggregator.AddNamedParameter(expression.Value);

            if (namedParameter.Value == null)
            {
                _expression.Append("NULL");
            }
            else if (namedParameter.Value is string)
            {
                _expression.AppendFormat("'{0}'", namedParameter.Value);
            }
            else if (namedParameter.Value is bool)
            {
                _expression.Append((bool) namedParameter.Value ? "TRUE" : "FALSE");
            }
            else if (namedParameter.Value is Array)
            {
                _expression.Append('[');

                bool first = true;
                foreach (var element in (System.Collections.IEnumerable)namedParameter.Value)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        _expression.Append(", ");
                    }

                    VisitConstantExpression(System.Linq.Expressions.Expression.Constant(element));
                }

                _expression.Append(']');
            }
            else
            {
                _expression.AppendFormat("{0}", namedParameter.Value);
            }

            return expression;
        }

        protected override Expression VisitConditionalExpression(ConditionalExpression expression)
        {
            _expression.Append("CASE WHEN ");
            VisitExpression(expression.Test);
            _expression.Append(" THEN ");
            VisitExpression(expression.IfTrue);
            _expression.Append(" ELSE ");
            VisitExpression(expression.IfFalse);
            _expression.Append(" END");

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

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    _expression.Append('-');
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
            var modelVisitor = new N1QlQueryModelVisitor(_methodCallTranslatorProvider);

            modelVisitor.VisitQueryModel(expression.QueryModel);
            _expression.Append(modelVisitor.GetQuery());

            return expression;
        }
    }
}