using System;
using System.Linq;
using System.Linq.Expressions;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// When using Linq2Couchbase, automatically filter returned entities by the "type" attribute
    /// </summary>
    public class EntityTypeFilterAttribute : EntityFilterAttribute
    {
        /// <summary>
        /// Filter the results to include entities with this string as the "type" attribute
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Creates a new EntityTypeFilterAttribute
        /// </summary>
        /// <param name="type">Filter the results to include entities with this string as the "type" attribute</param>
        public EntityTypeFilterAttribute(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Apply the filter to a LINQ query
        /// </summary>
        public override IEntityFilter<T> GetFilter<T>()
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
            
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.PropertyOrField(parameter, "type"), Expression.Constant(Type)), parameter);
        }

        private class WhereFilter<T> : IEntityFilter<T>
        {
            public Expression<Func<T, bool>> WhereExpression { get; set; }
            public int Priority { get; set; }

            public IQueryable<T> ApplyFilter(IQueryable<T> source)
            {
                return source.Where(WhereExpression);
            }
        }

    }
}
