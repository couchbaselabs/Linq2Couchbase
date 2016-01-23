using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Couchbase.Linq
{
    /// <summary>
    /// Represents date parts for calls to date related <see cref="N1QlFunctions"/>.
    /// </summary>
    /// <remarks>
    /// Different date related functions are compatible with different date parts.
    /// For details, see the N1QL documentation.
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum N1QlDatePart
    {
        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "millennium")]
        Millennium,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "century")]
        Century,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "decade")]
        Decade,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "year")]
        Year,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "quarter")]
        Quarter,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "month")]
        Month,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "week")]
        Week,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "day")]
        Day,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "hour")]
        Hour,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "minute")]
        Minute,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "second")]
        Second,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "millisecond")]
        Millisecond,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "doy")]
        DayOfYear,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "dow")]
        DayOfWeek,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "iso_week")]
        IsoWeek,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "iso_year")]
        IsoYear,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "iso_dow")]
        IsoDayOfWeek,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "timezone")]
        TimeZone,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "timezone_hour")]
        TimeZoneOffsetHour,

        /// <summary>
        /// See N1QL documentation.
        /// </summary>
        [EnumMember(Value = "timezone_minute")]
        TimeZoneOffsetMinute
    }
}
