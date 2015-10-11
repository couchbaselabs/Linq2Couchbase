using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Couchbase.Linq.QueryGeneration.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration
{
    internal class N1QlExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        private static readonly MethodInfo[] StringCompareMethods = {
           typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) }),
           typeof(string).GetMethod("CompareTo", new[] { typeof(string) })
        };

        private readonly StringBuilder _expression = new StringBuilder();
        public StringBuilder Expression
        {
            get { return _expression; }
        }

        private readonly N1QlQueryGenerationContext _queryGenerationContext;

        protected N1QlExpressionTreeVisitor(N1QlQueryGenerationContext queryGenerationContext)
        {
            if (queryGenerationContext == null)
            {
                throw new ArgumentNullException("queryGenerationContext");
            }

            _queryGenerationContext = queryGenerationContext;
        }

        public static string GetN1QlExpression(Expression expression, N1QlQueryGenerationContext queryGenerationContext)
        {
            // Ensure that any date/time expressions are properly converted to Unix milliseconds as needed
            expression = TransformingExpressionTreeVisitor.Transform(expression,
                ExpressionTransformers.DateTimeTransformationRegistry.Default);

            var visitor = new N1QlExpressionTreeVisitor(queryGenerationContext);
            visitor.VisitExpression(expression);
            return visitor.GetN1QlExpression();
        }

        public static string GetN1QlSelectNewExpression(NewExpression expression, N1QlQueryGenerationContext queryGenerationContext)
        {
            // Ensure that any date/time expressions are properly converted to Unix milliseconds as needed
            expression = (NewExpression)TransformingExpressionTreeVisitor.Transform(expression,
                ExpressionTransformers.DateTimeTransformationRegistry.Default);

            var visitor = new N1QlExpressionTreeVisitor(queryGenerationContext);
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

        public override Expression VisitExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Coalesce:
                    return VisitCoalesceExpression((BinaryExpression) expression);

                case ExpressionType.ArrayIndex:
                    return VisitArrayIndexExpression((BinaryExpression) expression);

                case ExpressionType.Extension:
                    return VisitExtensionExpression(expression);
                    
                default:
                    return base.VisitExpression(expression);
            }
        }

        protected Expression VisitExtensionExpression(Expression expression)
        {
            var stringComparison = expression as StringComparisonExpression;
            if (stringComparison != null)
            {
                return VisitStringComparisonExpression(stringComparison);
            }

            throw CreateUnhandledItemException(expression, "VisitExtensionExpression");
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
                    _expression.AppendFormat(" as {0}", N1QlHelpers.EscapeIdentifier(members[i].Name));
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
            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
            {
                var newExpression = ConvertStringCompareExpression(binaryExpression);
                if (newExpression != expression)
                {
                    // Stop processing the current expression, visit the new expression instead
                    return VisitExpression(newExpression);
                }
            }

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

        #region String Comparison

        /// <summary>
        /// Converts String.Compare expressions to a StringComparisonExpression, if applicable
        /// </summary>
        /// <param name="expression">BinaryExpression to test and convert</param>
        /// <returns>If not a String.Compare expression, returns the original expression.  Otherwise the converted expression.</returns>
        /// <remarks>
        /// Converts String.Compare and String.CompareTo clauses where compared to an integer.
        /// i.e. String.Compare(x, y) &lt; 0 or x.CompareTo(y) &lt; 0 are both the equivalent of x &lt; y
        /// </remarks>
        public Expression ConvertStringCompareExpression(BinaryExpression expression)
        {
            // Only convert <, <=, >, >=, ==, !=

            if (!StringComparisonExpression.SupportedOperations.Contains(expression.NodeType))
            {
                return expression;
            }

            // See if one side is a call to String.Compare

            var leftExpression = expression.Left as MethodCallExpression;
            var rightExpression = expression.Right as MethodCallExpression;

            if ((leftExpression != null) && !StringCompareMethods.Contains(leftExpression.Method))
            {
                leftExpression = null;
            }
            if ((rightExpression != null) && !StringCompareMethods.Contains(rightExpression.Method))
            {
                rightExpression = null;
            }

            var methodCallExpression = leftExpression ?? rightExpression;

            if (methodCallExpression == null)
            {
                // Not a string comparison
                return expression;
            }

            // Get the number side of the comparison, which must be a constant integer

            var numericExpression = leftExpression != null
                ? expression.Right as ConstantExpression
                : expression.Left as ConstantExpression;

            if ((numericExpression == null) || !typeof (int).IsAssignableFrom(numericExpression.Type))
            {
                // Only convert if comparing to an integer
                return expression;
            }

            var number = (int) numericExpression.Value;

            // Get the strings from the method call parameters

            Expression leftString;
            Expression rightString;

            if (methodCallExpression.Arguments.Count > 1)
            {
                leftString = methodCallExpression.Arguments[0];
                rightString = methodCallExpression.Arguments[1];
            }
            else
            {
                leftString = methodCallExpression.Object;
                rightString = methodCallExpression.Arguments[0];
            }

            if (leftExpression == null)
            {
                // If the method call is on the right side of the binary expression, then reverse the strings

                var temp = leftString;
                leftString = rightString;
                rightString = temp;
            }

            return ConvertStringCompareExpression(leftString, rightString, expression.NodeType, number);
        }

        /// <summary>
        /// Converts String.Compare expression to StringComparisonExpression
        /// </summary>
        /// <param name="leftString">String expression on the left side of the comparison</param>
        /// <param name="rightString">String expression on the right side of the comparison</param>
        /// <param name="operation">Comparison operation being performed</param>
        /// <param name="number">Number that String.Compare was being compared to, typically 0, 1, or -1.</param>
        private Expression ConvertStringCompareExpression(Expression leftString, Expression rightString,
            ExpressionType operation, int number)
        {
            if (number == 1)
            {
                if ((operation == ExpressionType.LessThan) || (operation == ExpressionType.NotEqual))
                {
                    operation = ExpressionType.LessThanOrEqual;
                }
                else if ((operation == ExpressionType.Equal || operation == ExpressionType.GreaterThanOrEqual))
                {
                    operation = ExpressionType.GreaterThan;
                }
                else
                {
                    // Always evaluates to true or false regardless of input, so return a constant expression
                    return System.Linq.Expressions.Expression.Constant(operation == ExpressionType.LessThanOrEqual,
                        typeof (bool));
                }
            } 
            else if (number == -1)
            {
                if ((operation == ExpressionType.GreaterThan) || (operation == ExpressionType.NotEqual))
                {
                    operation = ExpressionType.GreaterThanOrEqual;
                }
                else if ((operation == ExpressionType.Equal || operation == ExpressionType.LessThanOrEqual))
                {
                    operation = ExpressionType.LessThan;
                }
                else
                {
                    // Always evaluates to true or false regardless of input, so return a constant expression
                    return System.Linq.Expressions.Expression.Constant(operation == ExpressionType.GreaterThanOrEqual,
                        typeof(bool));
                }
            }
            else if (number > 1)
            {
                // Always evaluates to true or false regardless of input, so return a constant expression
                return System.Linq.Expressions.Expression.Constant(
                    (operation == ExpressionType.NotEqual) || (operation == ExpressionType.LessThan) || (operation == ExpressionType.LessThanOrEqual),
                    typeof(bool));
            }
            else if (number < -1)
            {
                // Always evaluates to true or false regardless of input, so return a constant expression
                return System.Linq.Expressions.Expression.Constant(
                    (operation == ExpressionType.NotEqual) || (operation == ExpressionType.GreaterThan) || (operation == ExpressionType.GreaterThanOrEqual),
                    typeof(bool));
            }
            
            // If number == 0 we just leave operation unchanged

            return StringComparisonExpression.Create(operation, leftString, rightString);
        }

        protected virtual Expression VisitStringComparisonExpression(StringComparisonExpression expression)
        {
            _expression.Append('(');
            VisitExpression(expression.Left);

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

            VisitExpression(expression.Right);
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
            VisitExpression(expression.Left);

            var rightExpression = expression.Right;
            while (rightExpression != null)
            {
                _expression.Append(", ");

                if (rightExpression.NodeType == ExpressionType.Coalesce)
                {
                    var subExpression = (BinaryExpression) rightExpression;
                    VisitExpression(subExpression.Left);

                    rightExpression = subExpression.Right;
                }
                else
                {
                    VisitExpression(rightExpression);
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
            VisitExpression(expression.Left);
            _expression.Append('[');
            VisitExpression(expression.Right);
            _expression.Append(']');

            return expression;
        }

        /// <summary>
        ///     Tries to translate the Method-call to some N1QL expression. Currently only implemented for "Contains() - LIKE"
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            IMethodCallTranslator methodCallTranslator = _queryGenerationContext.MethodCallTranslatorProvider.GetTranslator(expression);

            if (methodCallTranslator != null)
            {
                return methodCallTranslator.Translate(expression, this);
            }

            return base.VisitMethodCallExpression(expression);
        }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            var namedParameter = _queryGenerationContext.ParameterAggregator.AddNamedParameter(expression.Value);

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
                _expression.Append((bool) namedParameter.Value ? "TRUE" : "FALSE");
            }
            else if (namedParameter.Value is DateTime)
            {
                // For consistency, use the JSON serializer configured for the cluster
                var serializedDateTime = _queryGenerationContext.Serializer.Serialize(namedParameter.Value);

                _expression.Append(Encoding.UTF8.GetString(serializedDateTime));
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
            _expression.Append(_queryGenerationContext.ExtentNameProvider.GetExtentName(expression.ReferencedQuerySource));

            return expression;
        }

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            if (expression.Expression.Type.Assembly.GetName().Name == "mscorlib")
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

                    return VisitExpression(newExpression);
                }
            }

            string memberName;

            if (_queryGenerationContext.MemberNameResolver.TryResolveMemberName(expression.Member, out memberName))
            {
                VisitExpression(expression.Expression);
                _expression.AppendFormat(".{0}", N1QlHelpers.EscapeIdentifier(memberName));
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
            var modelVisitor = new N1QlQueryModelVisitor(_queryGenerationContext, true);

            modelVisitor.VisitQueryModel(expression.QueryModel);
            _expression.Append(modelVisitor.GetQuery());

            return expression;
        }
    }
}