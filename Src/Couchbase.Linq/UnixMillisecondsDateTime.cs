using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    /// <summary>
    /// Used internally during query generation to represent a DateTime in unix milliseconds format.
    /// This class is not instantiated or used, it only exists in Expression trees.
    /// </summary>
    internal struct UnixMillisecondsDateTime
    {
        private readonly DateTime _dateTime;

        private UnixMillisecondsDateTime(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof (UnixMillisecondsDateTime))
            {
                return false;
            }

            return _dateTime == ((UnixMillisecondsDateTime) obj)._dateTime;
        }

        public override int GetHashCode()
        {
            return _dateTime.GetHashCode();
        }

        public override string ToString()
        {
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            return _dateTime.ToString();
        }

        public static UnixMillisecondsDateTime FromDateTime(DateTime dateTime)
        {
            return new UnixMillisecondsDateTime(dateTime);
        }

        public static UnixMillisecondsDateTime? FromDateTime(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return new UnixMillisecondsDateTime(dateTime.Value);
            }
            else
            {
                return null;
            }
        }

        public static DateTime ToDateTime(UnixMillisecondsDateTime unixMillisecondsDateTime)
        {
            return unixMillisecondsDateTime._dateTime;
        }

        public static DateTime? ToDateTime(UnixMillisecondsDateTime? unixMillisecondsDateTime)
        {
            if (unixMillisecondsDateTime.HasValue)
            {
                return unixMillisecondsDateTime.Value._dateTime;
            }
            else
            {
                return null;
            }
        }

        public static bool operator ==(UnixMillisecondsDateTime left, UnixMillisecondsDateTime right)
        {
            return left._dateTime == right._dateTime;
        }

        public static bool operator !=(UnixMillisecondsDateTime left, UnixMillisecondsDateTime right)
        {
            return left._dateTime != right._dateTime;
        }

        public static bool operator >(UnixMillisecondsDateTime left, UnixMillisecondsDateTime right)
        {
            return left._dateTime > right._dateTime;
        }

        public static bool operator <(UnixMillisecondsDateTime left, UnixMillisecondsDateTime right)
        {
            return left._dateTime < right._dateTime;
        }

        public static bool operator >=(UnixMillisecondsDateTime left, UnixMillisecondsDateTime right)
        {
            return left._dateTime >= right._dateTime;
        }

        public static bool operator <=(UnixMillisecondsDateTime left, UnixMillisecondsDateTime right)
        {
            return left._dateTime <= right._dateTime;
        }

        //public static implicit operator UnixMillisecondsDateTime(DateTime dateTime)
        //{
        //    return FromDateTime(dateTime);
        //}

        //public static implicit operator DateTime(UnixMillisecondsDateTime unixMillisecondsDateTime)
        //{
        //    return ToDateTime(unixMillisecondsDateTime);
        //}
    }
}
