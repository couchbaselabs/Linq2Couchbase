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
        [EnumMember(Value = "millennium")]
        Millennium,

        [EnumMember(Value = "century")]
        Century,

        [EnumMember(Value = "decade")]
        Decade,

        [EnumMember(Value = "year")]
        Year,

        [EnumMember(Value = "quarter")]
        Quarter,

        [EnumMember(Value = "month")]
        Month,

        [EnumMember(Value = "week")]
        Week,

        [EnumMember(Value = "day")]
        Day,

        [EnumMember(Value = "hour")]
        Hour,

        [EnumMember(Value = "minute")]
        Minute,

        [EnumMember(Value = "second")]
        Second,

        [EnumMember(Value = "millisecond")]
        Millisecond,

        [EnumMember(Value = "doy")]
        DayOfYear,

        [EnumMember(Value = "dow")]
        DayOfWeek,

        [EnumMember(Value = "iso_week")]
        IsoWeek,

        [EnumMember(Value = "iso_year")]
        IsoYear,

        [EnumMember(Value = "iso_dow")]
        IsoDayOfWeek,

        [EnumMember(Value = "timezone")]
        TimeZone,

        [EnumMember(Value = "timezone_hour")]
        TimeZoneOffsetHour,

        [EnumMember(Value = "timezone_minute")]
        TimeZoneOffsetMinute
    }
}
