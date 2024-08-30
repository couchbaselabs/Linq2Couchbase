using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Utils;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// This is an enhanced version of the PartialEvaluatingExpressionVisitor that is able to evaluate
    /// more complex expressions without first compiling them to delegates. This significantly reduces the CPU and
    /// allocation overhead caused by JIT compilation. The optimizations used are particularly targeted to cover
    /// the most common scenario, which is accessing a local variable within a lambda which has been captured by
    /// the compiler in a closure object.
    /// </summary>
    internal sealed class EnhancedPartialEvaluatingExpressionVisitor : RelinqExpressionVisitor
    {
        public static Expression? EvaluateIndependentSubtrees(Expression expressionTree, IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            ThrowHelpers.ThrowIfNull(expressionTree);
            ThrowHelpers.ThrowIfNull(evaluatableExpressionFilter);

            var partialEvaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze(expressionTree, evaluatableExpressionFilter);

            var visitor = new EnhancedPartialEvaluatingExpressionVisitor(partialEvaluationInfo, evaluatableExpressionFilter);
            return visitor.Visit(expressionTree);
        }

        // _partialEvaluationInfo contains a list of the expressions that are safe to be evaluated.
        private readonly PartialEvaluationInfo _partialEvaluationInfo;
        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;

        private EnhancedPartialEvaluatingExpressionVisitor(
            PartialEvaluationInfo partialEvaluationInfo,
            IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            ThrowHelpers.ThrowIfNull(partialEvaluationInfo);
            ThrowHelpers.ThrowIfNull(evaluatableExpressionFilter);

            _partialEvaluationInfo = partialEvaluationInfo;
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
        }

        public override Expression? Visit(Expression? expression)
        {
            // Only evaluate expressions which do not use any of the surrounding parameter expressions. Don't evaluate
            // lambda expressions (even if you could), we want to analyze those later on.
            if (expression is null)
            {
                return null;
            }

            if (expression.NodeType == ExpressionType.Lambda || !_partialEvaluationInfo.IsEvaluatableExpression(expression))
            {
                return base.Visit(expression);
            }

            Expression? evaluatedExpression;
            try
            {
                evaluatedExpression = EvaluateSubtree(expression);
            }
            catch (Exception ex)
            {
                // Evaluation caused an exception. Skip evaluation of this expression and proceed as if it weren't evaluable.
                var baseVisitedExpression = base.Visit(expression);
                // Then wrap the result to capture the exception for the back-end.
                return new PartialEvaluationExceptionExpression(ex, baseVisitedExpression);
            }

            if (evaluatedExpression != expression && evaluatedExpression is not null)
            {
                return EvaluateIndependentSubtrees(evaluatedExpression, _evaluatableExpressionFilter);
            }

            return evaluatedExpression;
        }

        /// <summary>
        /// Evaluates an evaluatable <see cref="Expression"/> subtree, i.e. an independent expression tree that is compilable and executable
        /// without any data being passed in. The result of the evaluation is returned as a <see cref="ConstantExpression"/>; if the subtree
        /// is already a <see cref="ConstantExpression"/>, no evaluation is performed.
        /// </summary>
        /// <param name="subtree">The subtree to be evaluated.</param>
        /// <returns>A <see cref="ConstantExpression"/> holding the result of the evaluation.</returns>
        private Expression? EvaluateSubtree(Expression? subtree)
        {
            if (subtree is null)
            {
                return null;
            }

            if (subtree is ConstantExpression constantExpression)
            {
                var valueAsIQueryable = constantExpression.Value as IQueryable;
                if (valueAsIQueryable != null && valueAsIQueryable.Expression != constantExpression)
                {
                    return valueAsIQueryable.Expression;
                }

                // It is important to return the original constant expression here or the Visit method
                // above will create an infinite recursion.
                return constantExpression;
            }

            var value = EvaluateOrExecuteSubtreeValue(subtree);
            if (value is Expression expression)
            {
                return expression;
            }

            return Expression.Constant(value, subtree.Type);
        }


        // May return an Expression if it can't be evaluated, otherwise the constant value.
        private object? EvaluateOrExecuteSubtreeValue(Expression subtree)
        {
            var (value, success) = EvaluateSubtreeValue(subtree);
            if (success)
            {
                return value;
            }

            // Fallback to compiling a delegate
            Expression<Func<object>> lambdaWithoutParameters =
                Expression.Lambda<Func<object>>(Expression.Convert(subtree, typeof(object)));
            var compiledLambda = lambdaWithoutParameters.Compile();

            return compiledLambda();
        }

        // Optimizations to avoid compiling and executing delegates if possible by evaluating some
        // common scenarios directly.
        private (object? value, bool success) EvaluateSubtreeValue(Expression subtree)
        {
            if (subtree is null)
            {
                return (null, false);
            }

            switch (subtree)
            {
                case ConstantExpression constantExpression:
                    // We've reached a constant. In the most common scenario, this will be a closure object created
                    // by the compiler to capture variables in a lambda expression. However, it could also be a parameter
                    // to a method call or a static field or property.
                    return (constantExpression.Value, true);

                case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
                    when unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type:
                    // Drill into conversions to Nullable<T> from concrete T.
                    return EvaluateSubtreeValue(unaryExpression.Operand);

                case MemberExpression memberExpression:
                    {
                        // Evaluate member access expressions. This will often be accessing a local variable from a
                        // closure object created by the compiler.

                        // Evaluate the object instance, if any. This will be null for static fields and properties.
                        object? instanceValue = null;
                        if (memberExpression.Expression is not null)
                        {
                            (instanceValue, var success) = EvaluateSubtreeValue(memberExpression.Expression);
                            if (!success)
                            {
                                return (null, false);
                            }
                        }

                        try
                        {
                            switch (memberExpression.Member)
                            {
                                case FieldInfo fieldInfo:
                                    return (fieldInfo.GetValue(instanceValue), true);

                                case PropertyInfo propertyInfo:
                                    return (propertyInfo.GetValue(instanceValue), true);
                            }
                        }
                        catch
                        {
                            // Fall back to the delegate compilation behavior
                        }
                    }
                    break;

                case MethodCallExpression methodCallExpression:
                    {
                        // Evaluate method calls such as extension methods. Note that only method calls that were
                        // previously considered evaluatable by EvaluatableTreeFindingExpressionVisitor.Analyze will
                        // be evaluated here, otherwise the parent expression would not be evaluatable and this code
                        // would not be reached.

                        // Evaluate the object instance, if any. This will be null for static methods.
                        object? instanceValue = null;
                        if (methodCallExpression.Object is not null)
                        {
                            (instanceValue, var success) = EvaluateSubtreeValue(methodCallExpression.Object);
                            if (!success)
                            {
                                return (null, false);
                            }
                        }

                        var argumentValues = methodCallExpression.Arguments.Count == 0
                            ? Array.Empty<object?>()
                            : new object?[methodCallExpression.Arguments.Count];

                        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
                        {
                            var (argumentValue, success) = EvaluateSubtreeValue(methodCallExpression.Arguments[i]);
                            if (!success)
                            {
                                return (null, false);
                            }

                            argumentValues[i] = argumentValue;
                        }

                        try
                        {
                            return (methodCallExpression.Method.Invoke(instanceValue, argumentValues), true);
                        }
                        catch
                        {
                            // Fall back to the delegate compilation behavior
                        }
                    }
                    break;
            }

            return (null, false);
        }
    }
}
