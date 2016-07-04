using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Helpers for N1QL query generation
    /// </summary>
    internal static class N1QlHelpers
    {

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
    }
}
