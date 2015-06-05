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
        private Expression _expressionCache;

        private string _type;
        /// <summary>
        /// Filter the results to include entities with this string as the "type" attribute
        /// </summary>
        public string Type
        {
            get { return _type; }
            set
            {
                if (value != _type)
                {
                    _type = value;
                    _expressionCache = null;
                }
            }
        }

        /// <summary>
        /// Creates a new EntityTypeFilterAttribute
        /// </summary>
        /// <param name="type">Filter the results to include entities with this string as the "type" attribute</param>
        public EntityTypeFilterAttribute(string type)
        {
            _type = type;
        }

        /// <summary>
        /// Apply the filter to a LINQ query
        /// </summary>
        public override IQueryable<T> ApplyFilter<T>(IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (!string.IsNullOrEmpty(Type))
            {
                return source.Where(GetExpression<T>());
            }
            else
            {
                return source;
            }
        }

        private Expression<Func<T, bool>> GetExpression<T>()
        {
            if (_expressionCache != null)
            {
                return (Expression<Func<T, bool>>) _expressionCache;
            }

            var parameter = Expression.Parameter(typeof (T), "p");
            
            var expression = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.PropertyOrField(parameter, "type"), Expression.Constant(Type)), parameter);

            _expressionCache = expression;
            return expression;
        }

    }
}
