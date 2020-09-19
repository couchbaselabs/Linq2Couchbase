using System;
using System.Linq;
using System.Linq.Expressions;

#nullable enable

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// When using Linq2Couchbase, automatically filter returned documents by the "type" attribute.
    /// </summary>
    public class DocumentTypeFilterAttribute : DocumentFilterAttribute
    {
        /// <summary>
        /// Filter the results to include documents with this string as the "type" attribute.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Creates a new DocumentTypeFilterAttribute.
        /// </summary>
        /// <param name="type">Filter the results to include documents with this string as the "type" attribute.</param>
        public DocumentTypeFilterAttribute(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <inheritdoc />
        public override IDocumentFilter<T> CreateFilter<T>()
        {
            return new WhereFilter<T>
            {
                Priority = Priority,
                WhereExpression = GetExpression<T>()
            };
        }

        private Expression<Func<T, bool>> GetExpression<T>()
        {
            var parameter = Expression.Parameter(typeof (T), "p");

            return Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.PropertyOrField(parameter, "type"),
                    Expression.Constant(Type)),
                parameter);
        }

        private class WhereFilter<T> : IDocumentFilter<T>
        {
            public Expression<Func<T, bool>> WhereExpression { get; set; } = null!;
            public int Priority { get; set; }

            public IQueryable<T> ApplyFilter(IQueryable<T> source)
            {
                return source.Where(WhereExpression);
            }
        }
    }
}
