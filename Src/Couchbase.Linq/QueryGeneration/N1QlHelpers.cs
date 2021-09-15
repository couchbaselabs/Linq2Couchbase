using System;
using Couchbase.Linq.Utils;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Helpers for N1QL query generation
    /// </summary>
    internal static class N1QlHelpers
    {
        public const string DefaultScopeName = "_default";
        public const string DefaultCollectionName = "_default";

        /// <summary>
        ///     Escapes a N1QL identifier using tick (`) characters
        /// </summary>
        /// <param name="identifier">The identifier to format</param>
        /// <returns>An escaped identifier</returns>
        public static string EscapeIdentifier(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (identifier.IndexOf('`') >= 0)
            {
                // This should not occur, and is primarily in place to prevent N1QL injection attacks
                // So it isn't performance critical to perform this replace in a StringBuilder with the concatenation

                identifier = identifier.Replace("`", "``");
            }

            return string.Concat("`", identifier, "`");
        }

        /// <summary>
        /// Checks to see if the identifier may be a valid keyword.
        /// </summary>
        /// <param name="identifier">Identifier to check.</param>
        /// <returns>True if the identifier may be a valid keyword.</returns>
        /// <remarks>This method doesn't guarantee that they identifier is a currently known N1QL keyword, as this list
        /// may change over time.  It merely confirms that it is formatted as a plain string of alphabetic characters which
        /// may be a single keyword.  This provides security control against N1QL injection attacks where a single keyword
        /// is known to be safe but additional characters could be malicious.</remarks>
        public static bool IsValidKeyword(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return false;
            }

            for (var i = 0; i < identifier.Length; i++)
            {
                if (!char.IsLetter(identifier, i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Formats the bucket name and optionally the scope and collection names into an expression
        /// for inclusion in a N1QL query. This includes all required escaping.
        /// </summary>
        /// <param name="queryable">Collection being queried.</param>
        /// <returns>The expression string.</returns>
        public static string GetCollectionExpression(ICollectionQueryable queryable)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (queryable == null)
            {
                ThrowHelpers.ThrowArgumentNullException(nameof(queryable));
            }

            var bucketName = queryable.BucketName;
            var scopeName = queryable.ScopeName;
            var collectionName = queryable.CollectionName;

            return scopeName == DefaultScopeName && collectionName == DefaultCollectionName
                ? EscapeIdentifier(bucketName)
                : $"{EscapeIdentifier(queryable.BucketName)}.{EscapeIdentifier(scopeName)}.{EscapeIdentifier(collectionName)}";
        }
    }
}
