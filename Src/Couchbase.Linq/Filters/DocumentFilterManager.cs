using System;
using System.Collections.Generic;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Static class to cache and execute <see cref="IDocumentFilter{T}">IDocumentFilter</see> objects based on the type being queried
    /// </summary>
    class DocumentFilterManager
    {

        /// <summary>
        /// Stores currently loaded filters.
        /// </summary>
        /// <remarks>
        /// Any type which has no filters will be in the dictionary, with a value of null.  This will prevent another attempt
        /// to generate the default <see cref="DocumentFilterSet{T}">DocumentFilterSet</see> each time it is requested.
        /// </remarks>
        private static readonly Dictionary<Type, object> Filters = new Dictionary<Type, object>();

        private static IDocumentFilterSetGenerator _filterSetGenerator = new AttributeDocumentFilterSetGenerator();
        /// <summary>
        /// Generates the <see cref="DocumentFilterSet{T}">DocumentFilterSet</see> for a type if no filters have been previously loaded
        /// </summary>
        /// <remarks>By default, uses an <see cref="AttributeDocumentFilterSetGenerator">AttributeDocumentFeatureSetGenerator</see></remarks>
        public static IDocumentFilterSetGenerator FilterSetGenerator
        {
            get { return _filterSetGenerator; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _filterSetGenerator = value;
            }
        }

        /// <summary>
        /// Returns the filter set for a type, creating a new filters set using the <see cref="FilterSetGenerator">FilterSetGenerator</see> if there is no key in the Filters dictionary.
        /// </summary>
        /// <returns>Returns null if there are no filters defined for this type</returns>
        public static DocumentFilterSet<T> GetFilterSet<T>()
        {
            object filterSet;
            lock (Filters)
            {
                if (!Filters.TryGetValue(typeof(T), out filterSet))
                {
                    filterSet = FilterSetGenerator.GenerateDocumentFilterSet<T>();
                    Filters.Add(typeof(T), filterSet);
                }
            }

            return (DocumentFilterSet<T>)filterSet;
        }

        /// <summary>
        /// Add or change filter, replacing the entire filter set if present
        /// </summary>
        public static void SetFilter<T>(IDocumentFilter<T> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            lock (Filters)
            {
                Filters[typeof(T)] = new DocumentFilterSet<T>(filter);
            }
        }

        /// <summary>
        /// Add or change a filter set
        /// </summary>
        public static void SetFilterSet<T>(DocumentFilterSet<T> filterSet)
        {
            if (filterSet == null)
            {
                throw new ArgumentNullException("filterSet");
            }

            lock (Filters)
            {
                Filters[typeof(T)] = filterSet;
            }
        }

        /// <summary>
        /// Remove a filter set.
        /// </summary>
        public static void RemoveFilterSet<T>()
        {

            lock (Filters)
            {
                Filters[typeof(T)] = null;
            }
        }

        /// <summary>
        /// Clear all filter sets.
        /// </summary>
        /// <remarks>Will cause future requests to be regenerated using the <see cref="FilterSetGenerator">FilterSetGenerator</see>.</remarks>
        public static void Clear()
        {
            lock (Filters)
            {
                Filters.Clear();
            }
        }

        /// <summary>
        /// Apply filters to a LINQ query
        /// </summary>
        public static IQueryable<T> ApplyFilters<T>(IQueryable<T> source)
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
