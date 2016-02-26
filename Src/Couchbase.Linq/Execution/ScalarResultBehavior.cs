using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Defines the behaviors related to scalar results, which must typically be extracted
    /// from an attribute named "result".  This typically applies to aggregate operations, as well as
    /// <see cref="Queryable.Any{T}(IQueryable{T})"/> and <see cref="Queryable.All{T}"/> operations.
    /// </summary>
    internal class ScalarResultBehavior
    {
        /// <summary>
        /// If true, indicates that result extraction is required.  The query should be executed with a return type
        /// of <see cref="ScalarResult{T}"/>, and then passed through <see cref="ApplyResultExtraction{T}"/>.
        /// </summary>
        public bool ResultExtractionRequired { get; set; }

        /// <summary>
        /// If NoRowsResult is not null, and if no rows are returned, then a single row with this value is returned instead.
        /// This is used to return the expected results for <see cref="Queryable.Any{T}(IQueryable{T})"/> and
        /// <see cref="Queryable.All{T}"/> operations.
        /// </summary>
        public object NoRowsResult { get; set; }

        /// <summary>
        /// Applies result extraction to a collection of <see cref="ScalarResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Result type being extracted.</typeparam>
        /// <param name="source">Collection of <see cref="ScalarResult{T}"/>.</param>
        /// <returns>Collection of <typeparamref name="T"/>.</returns>
        public IEnumerable<T> ApplyResultExtraction<T>(IEnumerable<ScalarResult<T>> source)
        {
            var result = source.Select(p => p.result);

            if (NoRowsResult != null)
            {
                var typeCastNoRowsResult = (T) NoRowsResult;
                if (typeCastNoRowsResult == null)
                {
                    throw new InvalidOperationException(
                        string.Format("Cannot apply result extraction, NoRowsResult is not of type '{0}'",
                            typeof (T).FullName));
                }

                result = result.DefaultIfEmpty(typeCastNoRowsResult);
            }

            return result;
        }
    }
}
