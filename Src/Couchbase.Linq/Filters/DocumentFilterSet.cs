using System;
using System.Collections.Generic;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Stores a list of <see cref="IDocumentFilter{T}">IDocumentFilter</see>s, sorted by Priority
    /// </summary>
    /// <remarks>
    /// Sort order of IDocumentFilters with the same Priority is undefined
    /// </remarks>
    public class DocumentFilterSet<T> : SortedSet<IDocumentFilter<T>>
    {

        /// <summary>
        /// Create an empty DocumentFilterSet
        /// </summary>
        public DocumentFilterSet() : base(new PriorityComparer())
        {
        }

        /// <summary>
        /// Create an DocumentFilterSet, filled with a set of filters
        /// </summary>
        public DocumentFilterSet(IEnumerable<IDocumentFilter<T>> filters) : base(filters, new PriorityComparer())
        {   
        }

        /// <summary>
        /// Create an DocumentFilterSet, filled with a set of filters
        /// </summary>
        public DocumentFilterSet(params IDocumentFilter<T>[] filters)
            : base(filters, new PriorityComparer())
        {
        }

        /// <summary>
        /// Apply the filters to a LINQ query, in order
        /// </summary>
        public IQueryable<T> ApplyFilters(IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (var filter in this)
            {
                source = filter.ApplyFilter(source);
            }

            return source;
        }

        private class PriorityComparer : IComparer<IDocumentFilter<T>>
        {

            public int Compare(IDocumentFilter<T> x, IDocumentFilter<T> y)
            {
                return x.Priority.CompareTo(y.Priority);
            }

        }
    }
}
