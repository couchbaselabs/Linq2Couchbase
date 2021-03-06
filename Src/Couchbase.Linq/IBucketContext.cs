﻿using System;
using System.Linq;

namespace Couchbase.Linq
{
    /// <summary>
    /// Provides a single point of entry to a Couchbase collection which makes it easier to compose
    /// and execute queries and to group together changes which will be submitted back into the bucket.
    /// </summary>
    public interface IBucketContext
    {
        /// <summary>
        /// Gets the bucket the <see cref="IBucketContext"/> was created against.
        /// </summary>
        /// <value>The <see cref="IBucket"/>.</value>
        IBucket Bucket { get; }

        /// <summary>
        /// Query timeout, if null uses cluster default.
        /// </summary>
        TimeSpan? QueryTimeout { get; set; }

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type T. This is the target of
        /// a LINQ query and requires that the associated JSON document have a type property that is the same as T.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <returns><see cref="IQueryable{T}" /> which can be used to query the bucket.</returns>
        IQueryable<T> Query<T>();

        /// <summary>
        /// Queries the current <see cref="IBucket" /> for entities of type T. This is the target of
        /// a LINQ query and requires that the associated JSON document have a type property that is the same as T.
        /// </summary>
        /// <typeparam name="T">An entity or POCO representing the object graph of a JSON document.</typeparam>
        /// <param name="options">Options to control the returned query.</param>
        /// <returns><see cref="IQueryable{T}" /> which can be used to query the bucket.</returns>
        IQueryable<T> Query<T>(BucketQueryOptions options);
    }
}
