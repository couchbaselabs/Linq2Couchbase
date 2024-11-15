﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Linq.Utils;

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
        private readonly SortedSet<IDocumentFilter<T>> _sortedSet;

        /// <summary>
        /// Create an DocumentFilterSet, filled with a set of filters.
        /// </summary>
        public DocumentFilterSet(IEnumerable<IDocumentFilter<T>> filters)
        {
            ThrowHelpers.ThrowIfNull(filters);

            _sortedSet = new SortedSet<IDocumentFilter<T>>(filters, new PriorityComparer());
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
            ThrowHelpers.ThrowIfNull(source);

            foreach (var filter in this)
            {
                source = filter.ApplyFilter(source);
            }

            return source;
        }

        /// <inheritdoc />
        public IEnumerator<IDocumentFilter<T>> GetEnumerator() => _sortedSet.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class PriorityComparer : IComparer<IDocumentFilter<T>>
        {
            public int Compare(IDocumentFilter<T>? x, IDocumentFilter<T>? y)
            {
                if (x is null)
                {
                    return y is null ? 0 : -1;
                }
                else if (y is null)
                {
                    return 1;
                }

                return x.Priority.CompareTo(y.Priority);
            }
        }
    }
}
