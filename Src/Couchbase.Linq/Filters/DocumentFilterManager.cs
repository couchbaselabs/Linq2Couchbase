using System;
using System.Collections.Concurrent;
using System.Linq;

#nullable enable

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Caches and execute <see cref="IDocumentFilter{T}">IDocumentFilter</see> objects based on the type being queried.
    /// This class is thread-safe.
    /// </summary>
    public class DocumentFilterManager
    {
        /// <summary>
        /// Stores currently loaded filters.
        /// </summary>
        /// <remarks>
        /// Any type which has no filters will be in the dictionary, with a value of null. This will prevent another attempt
        /// to generate the default <see cref="DocumentFilterSet{T}" /> each time it is requested.
        /// </remarks>
        private readonly ConcurrentDictionary<Type, object?> _filters = new ConcurrentDictionary<Type, object?>();

        private IDocumentFilterSetGenerator _filterSetGenerator = new AttributeDocumentFilterSetGenerator();

        /// <summary>
        /// Generates the <see cref="DocumentFilterSet{T}">DocumentFilterSet</see> for a type if no filters have been previously loaded.
        /// </summary>
        /// <remarks>By default, uses an <see cref="AttributeDocumentFilterSetGenerator" />.</remarks>
        public IDocumentFilterSetGenerator FilterSetGenerator
        {
            get => _filterSetGenerator;
            set => _filterSetGenerator = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns the filter set for a type, creating a new filters set using the <see cref="DocumentFilterManager.FilterSetGenerator" />
        /// if there is no key in the Filters dictionary.
        /// </summary>
        /// <returns>Returns null if there are no filters defined for this type</returns>
        public DocumentFilterSet<T>? GetFilterSet<T>()
        {
            return (DocumentFilterSet<T>?) _filters.GetOrAdd(typeof(T),
                key => FilterSetGenerator.GenerateDocumentFilterSet<T>());
        }

        /// <summary>
        /// Add or change filter, replacing the entire filter set if present
        /// </summary>
        public void SetFilter<T>(IDocumentFilter<T> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            SetFilterSet(new DocumentFilterSet<T>(filter));
        }

        /// <summary>
        /// Add or change a filter set.
        /// </summary>
        public void SetFilterSet<T>(DocumentFilterSet<T> filterSet)
        {
            _filters[typeof(T)] = filterSet ?? throw new ArgumentNullException(nameof(filterSet));
        }

        /// <summary>
        /// Remove a filter set.
        /// </summary>

        public void RemoveFilterSet<T>()
        {
            _filters[typeof(T)] = null;
        }

        /// <summary>
        /// Clear all filter sets.
        /// </summary>
        /// <remarks>Will cause future requests to be regenerated using the <see cref="DocumentFilterManager.FilterSetGenerator" />.</remarks>
        public void Clear()
        {
            _filters.Clear();
        }

        /// <summary>
        /// Apply filters to a LINQ query.
        /// </summary>
        public IQueryable<T> ApplyFilters<T>(IQueryable<T> source)
        {
            var filterSet = GetFilterSet<T>();

            if (filterSet != null)
            {
                return filterSet.ApplyFilters(source);
            }
            else
            {
                return source;
            }
        }
    }
}
