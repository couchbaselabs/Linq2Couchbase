using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Couchbase.Linq.Filters;
using Couchbase.Linq.Utils;
using Remotion.Linq.Utilities;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Implementation of <see cref="IQueryProvider"/> which applies filters to <see cref="IDocumentSet"/> instances
    /// as each query is executed. This allows a long-lived <see cref="IQueryProvider"/> which still functions correctly
    /// if document filters are changed during its lifetime.
    /// </summary>
    internal sealed class DelayedFilterQueryProvider : IAsyncQueryProvider
    {
        private static readonly MethodInfo s_genericCreateQueryMethod =
            ((Func<DelayedFilterQueryProvider, Expression, IQueryable<object>>)CreateQuery<object>).Method.GetGenericMethodDefinition();

        private readonly IAsyncQueryProvider _innerQueryProvider;
        private readonly ApplyFiltersExpressionVisitor _applyFiltersExpressionVisitor;

        public DelayedFilterQueryProvider(IAsyncQueryProvider innerQueryProvider, DocumentFilterManager filterManager)
        {
            ThrowHelpers.ThrowIfNull(innerQueryProvider);
            ThrowHelpers.ThrowIfNull(filterManager);

            _innerQueryProvider = innerQueryProvider;
            _applyFiltersExpressionVisitor = new ApplyFiltersExpressionVisitor(filterManager);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            ThrowHelpers.ThrowIfNull(expression);

            if (!ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable(expression.Type, out var itemType))
            {
                throw new ArgumentException($"Expected a closed generic type implementing IEnumerable<T>, but found '{expression.Type}'.", nameof(expression));
            }

            return (IQueryable)s_genericCreateQueryMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { this, expression })!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            ThrowHelpers.ThrowIfNull(expression);
            return CreateQuery<TElement>(this, expression);
        }

        private static IQueryable<TElement> CreateQuery<TElement>(DelayedFilterQueryProvider provider, Expression expression) =>
            new CouchbaseQueryable<TElement>(provider, expression);

        public object? Execute(Expression expression)
        {
            ThrowHelpers.ThrowIfNull(expression);

            return _innerQueryProvider.Execute(ApplyFilters(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            ThrowHelpers.ThrowIfNull(expression);

            return _innerQueryProvider.Execute<TResult>(ApplyFilters(expression));
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            ThrowHelpers.ThrowIfNull(expression);

            return _innerQueryProvider.ExecuteAsync<TResult>(ApplyFilters(expression), cancellationToken);
        }

        // Apply filters to IDocumentSet constants on-demand for each query in case they are modified between queries
        private Expression ApplyFilters(Expression expression) => _applyFiltersExpressionVisitor.Visit(expression);

        /// <summary>
        /// Visits the expression tree finding the innermost <see cref="IDocumentSet"/> constants
        /// and replaces them with new Queryable instances that apply the filters. Because a query may
        /// include multiple extents of multiple types, this visitor must be able to handle multiple different
        /// types for T.
        /// </summary>
        private sealed class ApplyFiltersExpressionVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo s_genericApplyFiltersMethod =
                ((Func<DocumentFilterManager, IQueryable<object>, IQueryable<object>>)ApplyFilters).Method.GetGenericMethodDefinition();

            private readonly DocumentFilterManager _filterManager;

            public ApplyFiltersExpressionVisitor(DocumentFilterManager filterManager)
            {
                _filterManager = filterManager;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is IDocumentSet documentSet)
                {
                    var filterMethod = s_genericApplyFiltersMethod.MakeGenericMethod(documentSet.ElementType);
                    var filteredQueryable = filterMethod.Invoke(null, new object[] { _filterManager, documentSet })!;

                    if (!ReferenceEquals(documentSet, filteredQueryable))
                    {
                        // Replace the constant with a new queryable that applies the filters
                        // if a different queryable is returned.

                        return ((IQueryable) filteredQueryable).Expression;
                    }
                }

                return node;
            }

            private static IQueryable<T> ApplyFilters<T>(DocumentFilterManager filterManager, IQueryable<T> source)
            {
                var filters = filterManager.GetFilterSet<T>();
                if (filters is null)
                {
                    return source;
                }

                return filters.ApplyFilters(source);
            }
        }
    }
}
