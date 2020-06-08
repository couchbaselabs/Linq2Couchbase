using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Couchbase.Core.IO.Serializers;
using Couchbase.Linq.QueryGeneration.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Couchbase.Linq.QueryGeneration
{
    internal class N1QlExpressionTreeVisitor : ThrowingExpressionVisitor, IN1QlExpressionTreeVisitor
    {
        private static readonly Assembly Mscorlib = typeof(string).GetTypeInfo().Assembly;

        private readonly StringBuilder _expression = new StringBuilder();
        public StringBuilder Expression => _expression;

        public N1QlQueryGenerationContext QueryGenerationContext { get; }

        protected N1QlExpressionTreeVisitor(N1QlQueryGenerationContext queryGenerationContext)
        {
            QueryGenerationContext = queryGenerationContext ?? throw new ArgumentNullException(nameof(queryGenerationContext));
        }

        public static string GetN1QlExpression(Expression expression, N1QlQueryGenerationContext queryGenerationContext)
        {
            var visitor = new N1QlExpressionTreeVisitor(queryGenerationContext);
            visitor.Visit(expression);
            return visitor.GetN1QlExpression();
        }

        public static string GetN1QlSelectNewExpression(Expression expression, N1QlQueryGenerationContext queryGenerationContext)
        {
            var visitor = new N1QlExpressionTreeVisitor(queryGenerationContext);
            visitor.VisitSelectNewExpression(expression);
            return visitor.GetN1QlExpression();
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            // No longer using FormattingExpressionTreeVisitor to format unhandledItem if it is an Expression
            // This visitor was removed in Relinq 2.0 as obsolete because ToString is now sufficient in .Net 4
            // https://www.re-motion.org/jira/browse/RMLNQ-56

            var message = string.Format(
                "The expression '{0}' (type: {1}) is not supported by this LINQ provider."
                , unhandledItem
                , typeof(T));

            return new NotSupportedException(message);
        }

        public string GetN1QlExpression()
        {
            return _expression.ToString();
        }

        /// <inheritdoc/>
        void IN1QlExpressionTreeVisitor.Visit(Expression expression)
        {
            Visit(expression);
        }

        public override Expression Visit(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Coalesce:
                    return VisitCoalesceExpression((BinaryExpression)expression);

                case ExpressionType.ArrayIndex:
                    return VisitArrayIndexExpression((BinaryExpression)expression);

                default:
                    return base.Visit(expression);
            }
        }

        protected override Expression VisitExtension(Expression expression)
        {
            var stringComparison = expression as StringComparisonExpression;
            if (stringComparison != null)
            {
                return VisitStringComparisonExpression(stringComparison);
            }

            return base.VisitExtension(expression);
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            VisitNewObject(expression.Members, expression.Arguments);

            return expression;
        }

        protected override Expression VisitMemberInit(MemberInitExpression expression)
        {
            if (expression.NewExpression.Arguments.Count > 0)
            {
                throw new NotSupportedException("New Objects Must Be Initialized With A Parameterless Constructor");
            }
            if (expression.Bindings.Any(p => p.BindingType != MemberBindingType.Assignment))
            {
                throw new NotSupportedException("New Objects Must Be Initialized With Assignments Only");
            }

            var arguments = expression.Bindings.Cast<MemberAssignment>().Select(p => p.Expression).ToList();
            var members = expression.Bindings.Select(p => p.Member).ToList();

            VisitNewObject(members, arguments);

            return expression;
        }

        /// <summary>
        /// Shared logic for NewExpression and MemberInitExpression
        /// </summary>
        private void VisitNewObject(IList<MemberInfo> members, IList<Expression> arguments)
        {
            _expression.Append('{');

            for (var i = 0; i < members.Count; i++)
            {
                var beforeAppendLength = _expression.Length;

                if (i > 0)
                {
                    _expression.Append(", ");
                }

                if (!QueryGenerationContext.MemberNameResolver.TryResolveMemberName(members[i], out var memberName))
                {
                    memberName = members[i].Name;
                }

                _expression.AppendFormat("\"{0}\": ", memberName);

                var beforeSubExpressionLength = _expression.Length;

                Visit(arguments[i]);

                if (_expression.Length == beforeSubExpressionLength)
                {
                    // nothing was added for the value, so remove the part that was added originally
                    _expression.Length = beforeAppendLength;
                }
            }

            _expression.Append('}');
        }

        /// <summary>
        /// Parses the new object that is part of the select expression with "as" based formatting.
        /// Can accept either a NewExpression or MemberInitExpression
        /// </summary>
        private Expression VisitSelectNewExpression(Expression expression)
        {
            IList<Expression> arguments;
            IList<MemberInfo> members;

            var memberInitExpression = expression as MemberInitExpression;
            if (memberInitExpression != null)
            {
                if (memberInitExpression.NewExpression.Arguments.Count > 0)
                {
                    throw new NotSupportedException("New Objects Must Be Initialized With A Parameterless Constructor");
                }
                if (memberInitExpression.Bindings.Any(p => p.BindingType != MemberBindingType.Assignment))
                {
                    throw new NotSupportedException("New Objects Must Be Initialized With Assignments Only");
                }

                arguments = memberInitExpression.Bindings.Cast<MemberAssignment>().Select(p => p.Expression).ToList();
                members = memberInitExpression.Bindings.Select(p => p.Member).ToList();
            }
            else
            {
                var newExpression = expression as NewExpression;
                if (newExpression == null)
                {
                    throw new NotSupportedException("Unsupported Select Clause Expression");
                }

                arguments = newExpression.Arguments;
                members = newExpression.Members;
            }

            for (var i = 0; i < members.Count; i++)
            {
                if (i > 0)
                {
                    _expression.Append(", ");
                }

                var expressionLength = _expression.Length;

                Visit(arguments[i]);

                //only add 'as' part if the  previous visitexpression has generated something.
                if (_expression.Length > expressionLength)
                {
                    if (!QueryGenerationContext.MemberNameResolver.TryResolveMemberName(members[i], out var memberName))
                    {
                        memberName = members[i].Name;
                    }

                    _expression.AppendFormat(" as {0}", N1QlHelpers.EscapeIdentifier(memberName));
                }
                else if (i > 0)
                {
                    // nothing was added, so remove the extra comma
                    _expression.Length -= 2;
                }
            }

            return expression;
        }

        protected override Expression VisitNewArray(NewArrayExpression expression)
        {
            if (expression.NodeType == ExpressionType.NewArrayInit)
            {
                _expression.Append('[');

                for (var i = 0; i < expression.Expressions.Count; i++)
                {
                    if (i > 0)
                    {
                        _expression.Append(", ");
                    }

                    Visit(expression.Expressions[i]);
                }

                _expression.Append(']');

                return expression;
            }
            else
            {
                return base.VisitNewArray(expression);
            }
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            ConstantExpression constantExpression;

            _expression.Append("(");

            Visit(expression.Left);

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
                    if ((expression.Left.Type == typeof(string)) || (expression.Right.Type == typeof(string)))
                    {
                        _expression.Append(" || ");
                    }
                    else
                    {
                        _expression.Append(" + ");
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
                    base.VisitBinary(expression);
                    break;
            }

            Visit(expression.Right);
            _expression.Append(")");

            return expression;
        }

        #region String Comparison

        protected virtual Expression VisitStringComparisonExpression(StringComparisonExpression expression)
        {
            var toLower = expression.Comparison.HasValue && expression.Comparison.Value == StringComparison.OrdinalIgnoreCase;

            _expression.Append('(');
            if (toLower)
            {
                _expression.Append("LOWER(");
                Visit(expression.Left);
                _expression.Append(')');
            }
            else
            {
                Visit(expression.Left);
            }


            switch (expression.Operation)
            {
                case ExpressionType.Equal:
                    _expression.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    _expression.Append(" != ");
                    break;
                case ExpressionType.LessThan:
                    _expression.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _expression.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _expression.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _expression.Append(" >= ");
                    break;
            }

            if (toLower)
            {
                _expression.Append("LOWER(");
                Visit(expression.Right);
                _expression.Append(')');
            }
            else
            {
                Visit(expression.Right);
            }

            _expression.Append(')');

            return expression;
        }

        #endregion

        /// <summary>
        ///     Visits a coalese expression recursively, building a IFMISSINGORNULL function
        /// </summary>
        private Expression VisitCoalesceExpression(BinaryExpression expression)
        {
            _expression.Append("IFMISSINGORNULL(");
            Visit(expression.Left);

            var rightExpression = expression.Right;
            while (rightExpression != null)
            {
                _expression.Append(", ");

                if (rightExpression.NodeType == ExpressionType.Coalesce)
                {
                    var subExpression = (BinaryExpression)rightExpression;
                    Visit(subExpression.Left);

                    rightExpression = subExpression.Right;
                }
                else
                {
                    Visit(rightExpression);
                    rightExpression = null;
                }
            }

            _expression.Append(')');

            return expression;
        }

        /// <summary>
        ///     Special handling for ArrayIndex binary expressions
        /// </summary>
        protected virtual Expression VisitArrayIndexExpression(BinaryExpression expression)
        {
            Visit(expression.Left);
            _expression.Append('[');
            Visit(expression.Right);
            _expression.Append(']');

            return expression;
        }

        /// <summary>
        ///     Tries to translate the Method-call to some N1QL expression. Currently only implemented for "Contains() - LIKE"
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            IMethodCallTranslator methodCallTranslator = QueryGenerationContext.MethodCallTranslatorProvider.GetTranslator(expression);

            if (methodCallTranslator != null)
            {
                return methodCallTranslator.Translate(expression, this);
            }

            return base.VisitMethodCall(expression);
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var namedParameter = QueryGenerationContext.ParameterAggregator.AddNamedParameter(expression.Value);

            if (namedParameter.Value == null)
            {
                _expression.Append("NULL");
            }
            else if (namedParameter.Value is string)
            {
                _expression.AppendFormat("'{0}'", namedParameter.Value.ToString().Replace("'", "''"));
            }
            else if (namedParameter.Value is char)
            {
                _expression.AppendFormat("'{0}'", (char)namedParameter.Value != '\'' ? namedParameter.Value : "''");
            }
            else if (namedParameter.Value is bool)
            {
                _expression.Append((bool)namedParameter.Value ? "TRUE" : "FALSE");
            }
            else if (namedParameter.Value is DateTime || namedParameter.Value is DateTimeOffset)
            {
                // For consistency, use the JSON serializer configured for the cluster
                var serializedDateTime = QueryGenerationContext.Serializer.Serialize(namedParameter.Value);

                _expression.Append(Encoding.UTF8.GetString(serializedDateTime));
            }
            else if (namedParameter.Value is System.Collections.IEnumerable)
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

                    VisitConstant(System.Linq.Expressions.Expression.Constant(element));
                }

                _expression.Append(']');
            }
            else if (namedParameter.Value.GetType().GetTypeInfo().IsEnum)
            {
                // Use the type serializer to format the value as JSON.
                // This allows different serialization converters to work on the value.
                // Then convert back to get the raw type being stored, such as a string or integer.
                // Then we can write this value to N1QL by visiting it as a constant expression.

                var jsonString = QueryGenerationContext.Serializer.Serialize(namedParameter.Value);
                var jsonValue = QueryGenerationContext.Serializer.Deserialize<object>(jsonString);

                return Visit(System.Linq.Expressions.Expression.Constant(jsonValue));
            }
            else if (namedParameter.Value is Guid)
            {
                _expression.AppendFormat("'{0}'", namedParameter.Value.ToString());
            }
            else
            {
                // Use the invariant culture so that decimal handling is correct
                _expression.Append(Convert.ToString(namedParameter.Value, System.Globalization.CultureInfo.InvariantCulture));
            }

            return expression;
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            _expression.Append("CASE WHEN ");
            Visit(expression.Test);
            _expression.Append(" THEN ");
            Visit(expression.IfTrue);
            _expression.Append(" ELSE ");
            Visit(expression.IfFalse);
            _expression.Append(" END");

            return expression;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            _expression.Append(QueryGenerationContext.ExtentNameProvider.GetExtentName(expression.ReferencedQuerySource));

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression.Type.GetTypeInfo().Assembly.Equals(Mscorlib))
            {
                // For property getters on the core .Net classes, we don't want to just recurse through the .Net object model
                // Instead, convert to a MethodCallExpression
                // And it will pass through the appropriate IMethodCallTranslator
                // (i.e. string.Length)

                var propInfo = expression.Member as PropertyInfo;
                if ((propInfo != null) && (propInfo.GetMethod != null) && (propInfo.GetMethod.GetParameters().Length == 0))
                {
                    // Convert to a property getter method call
                    var newExpression = System.Linq.Expressions.Expression.Call(
                        expression.Expression,
                        propInfo.GetMethod);

                    return Visit(newExpression);
                }
            }

            if (QueryGenerationContext.MemberNameResolver.TryResolveMemberName(expression.Member, out var memberName))
            {
                var querySourceExpression = expression.Expression as QuerySourceReferenceExpression;
                if ((querySourceExpression != null) &&
                    (QueryGenerationContext.ExtentNameProvider.GetExtentName(
                        querySourceExpression.ReferencedQuerySource) == ""))
                {
                    // This query source has a blank extent name, so we don't need to reference the extent to access the member

                    _expression.Append(N1QlHelpers.EscapeIdentifier(memberName));
                }
                else
                {
                    Visit(expression.Expression);
                    _expression.AppendFormat(".{0}", N1QlHelpers.EscapeIdentifier(memberName));
                }
            }

            return expression;
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    _expression.Append("NOT ");
                    Visit(expression.Operand);
                    break;

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    _expression.Append('-');
                    Visit(expression.Operand);
                    break;

                default:
                    Visit(expression.Operand);
                    break;
            }
            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var modelVisitor = new N1QlQueryModelVisitor(QueryGenerationContext, true);

            modelVisitor.VisitQueryModel(expression.QueryModel);
            _expression.Append(modelVisitor.GetQuery());

            return expression;
        }
    }
}