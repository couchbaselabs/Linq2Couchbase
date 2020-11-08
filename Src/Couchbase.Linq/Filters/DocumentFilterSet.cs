using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Stores a list of <see cref="IDocumentFilter{T}"/>, sorted by <see cref="IDocumentFilter{T}.Priority"/>.
    /// </summary>
    /// <remarks>
    /// Sort order of filters with the same Priority is undefined. This set is immutable.
    /// </remarks>
    public class DocumentFilterSet<T> : IEnumerable<IDocumentFilter<T>>
    {
        private readonly SortedSet<IDocumentFilter<T>> _sortedSet =
            new SortedSet<IDocumentFilter<T>>(new PriorityComparer());

        /// <summary>
        /// Create an DocumentFilterSet, filled with a set of filters.
        /// </summary>
        public DocumentFilterSet(IEnumerable<IDocumentFilter<T>> filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            foreach (var filter in filters)
            {
                _sortedSet.Add(filter);
            }
        }

        /// <summary>
        /// Create an DocumentFilterSet, filled with a set of filters.
        /// </summary>
        public DocumentFilterSet(params IDocumentFilter<T>[] filters)
            : this((IEnumerable<IDocumentFilter<T>>) filters)
        {
        }

        /// <summary>
        /// Apply the filters to a LINQ query, in order.
        /// </summary>
        public IQueryable<T> ApplyFilters(IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var filter in this)
            {
                source = filter.ApplyFilter(source);
            }

            return source;
        }

        public IEnumerator<IDocumentFilter<T>> GetEnumerator() => _sortedSet.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class PriorityComparer : IComparer<IDocumentFilter<T>>
        {
            public int Compare(IDocumentFilter<T> x, IDocumentFilter<T> y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }
    }
}
