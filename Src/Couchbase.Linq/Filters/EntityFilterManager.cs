using System;
using System.Collections.Generic;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Static class to cache and execute IEntityFilters based on the type being queried
    /// </summary>
    class EntityFilterManager
    {

        /// <summary>
        /// Stores currently loaded filters.
        /// </summary>
        /// <remarks>
        /// Any type which has no filters will be in the dictionary, with a value of null.  This will prevent another attempt
        /// to generate the default <see cref="EntityFilterSet">EntityFilterSet</see> each time it is requested.
        /// </remarks>
        private static readonly Dictionary<Type, EntityFilterSet> Filters = new Dictionary<Type, EntityFilterSet>();

        private static IEntityFilterSetGenerator _filterSetGenerator = new AttributeEntityFilterSetGenerator();
        /// <summary>
        /// Generates the <see cref="EntityFilterSet">EntityFilterSet</see> for a type if no filters have been previously loaded
        /// </summary>
        /// <remarks>By default, uses an <see cref="AttributeEntityFilterSetGenerator">AttributeEntityFeatureSetGenerator</see></remarks>
        public static IEntityFilterSetGenerator FilterSetGenerator
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
        public static EntityFilterSet GetFilterSet(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            EntityFilterSet filterSet;
            lock (Filters)
            {
                if (!Filters.TryGetValue(type, out filterSet))
                {
                    filterSet = FilterSetGenerator.GenerateEntityFilterSet(type);
                    Filters.Add(type, filterSet);
                }
            }

            return filterSet;
        }

        /// <summary>
        /// Add or change a filter set
        /// </summary>
        public static void SetFilterSet(Type type, EntityFilterSet filterSet)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (filterSet == null)
            {
                throw new ArgumentNullException("filterSet");
            }

            lock (Filters)
            {
                Filters[type] = filterSet;
            }
        }

        /// <summary>
        /// Remove a filter set.
        /// </summary>
        public static void RemoveFilterSet(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            lock (Filters)
            {
                Filters[type] = null;
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
            var filterSet = GetFilterSet(typeof(T));

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
