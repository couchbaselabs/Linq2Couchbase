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
    public static class N1QlHelpers
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
                // So it isn't performance critical to perform this replace in the StringBuilder

                identifier = identifier.Replace("`", "``");
            }

            var sb = new StringBuilder(identifier.Length + 2);

            sb.Append('`');
            sb.Append(identifier);
            sb.Append('`');

            return sb.ToString();
        }

    }
}
