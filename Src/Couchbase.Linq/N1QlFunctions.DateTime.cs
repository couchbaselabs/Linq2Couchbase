using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    public partial class N1QlFunctions
    {
        /// <summary>
        /// Adds an interval to a date/time, where the unit of interval is part.
        /// </summary>
        /// <param name="date">Date/time on which to perform arithmetic.</param>
        /// <param name="interval">Interval to add to date.</param>
        /// <param name="part">Unit of the interval being added to date.</param>
        /// <returns>New date/time</returns>
        /// <remarks>Only valid for use in N1QL queries.</remarks>
        [N1QlFunction("DATE_ADD_STR")]
        public static DateTime DateAdd(DateTime date, long interval, N1QlDatePart part)
        {
            throw NotSupportedError();
        }

        /// <summary>
        /// Returns the elapsed time between date/times as an integer whose unit is part.
        /// </summary>
        /// <param name="date1">Starting date/time for difference.</param>
        /// <param name="date2">Ending date/time for difference.</param>
        /// <param name="part">Unit of the interval to return.</param>
        /// <returns>Difference between date1 and date2 in part units.  Result is positive if date1 is later than date2.</returns>
        /// <remarks>Only valid for use in N1QL queries.</remarks>
        [N1QlFunction("DATE_DIFF_STR")]
        public static long DateDiff(DateTime date1, DateTime date2, N1QlDatePart part)
        {
            throw NotSupportedError();
        }

        /// <summary>
        /// Returns the date part as an integer.
        /// </summary>
        /// <param name="date">Date/time to extract the part of</param>
        /// <param name="part">Part to extract.</param>
        /// <returns>Portion of the date/time, based on part.</returns>
        /// <remarks>Only valid for use in N1QL queries.</remarks>
        [N1QlFunction("DATE_PART_STR")]
        public static long DatePart(DateTime date, N1QlDatePart part)
        {
            throw NotSupportedError();
        }

        /// <summary>
        /// Truncates the given date/time so that the given date part is the least significant.
        /// </summary>
        /// <param name="date">Date/time to be truncated.</param>
        /// <param name="part">Part to be the least significant.</param>
        /// <returns>Truncated date/time.</returns>
        /// <remarks>Only valid for use in N1QL queries.</remarks>
        [N1QlFunction("DATE_TRUNC_STR")]
        public static DateTime DateTrunc(DateTime date, N1QlDatePart part)
        {
            throw NotSupportedError();
        }
    }
}
