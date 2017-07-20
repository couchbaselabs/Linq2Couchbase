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
    internal struct UnixMillisecondsDateTimeOffset
    {
        private readonly DateTimeOffset _dateTimeOffset;

        private UnixMillisecondsDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            _dateTimeOffset = dateTimeOffset;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof (UnixMillisecondsDateTimeOffset))
            {
                return false;
            }

            return _dateTimeOffset == ((UnixMillisecondsDateTimeOffset) obj)._dateTimeOffset;
        }

        public override int GetHashCode()
        {
            return _dateTimeOffset.GetHashCode();
        }

        public override string ToString()
        {
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            return _dateTimeOffset.ToString();
        }

        public static UnixMillisecondsDateTimeOffset FromDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            return new UnixMillisecondsDateTimeOffset(dateTimeOffset);
        }

        public static UnixMillisecondsDateTimeOffset? FromDateTimeOffset(DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset.HasValue)
            {
                return new UnixMillisecondsDateTimeOffset(dateTimeOffset.Value);
            }
            else
            {
                return null;
            }
        }

        public static DateTimeOffset ToDateTimeOffset(UnixMillisecondsDateTimeOffset unixMillisecondsDateTimeOffset)
        {
            return unixMillisecondsDateTimeOffset._dateTimeOffset;
        }

        public static DateTimeOffset? ToDateTimeOffset(UnixMillisecondsDateTimeOffset? unixMillisecondsDateTimeOffset)
        {
            if (unixMillisecondsDateTimeOffset.HasValue)
            {
                return unixMillisecondsDateTimeOffset.Value._dateTimeOffset;
            }
            else
            {
                return null;
            }
        }

        public static bool operator ==(UnixMillisecondsDateTimeOffset left, UnixMillisecondsDateTimeOffset right)
        {
            return left._dateTimeOffset == right._dateTimeOffset;
        }

        public static bool operator !=(UnixMillisecondsDateTimeOffset left, UnixMillisecondsDateTimeOffset right)
        {
            return left._dateTimeOffset != right._dateTimeOffset;
        }

        public static bool operator >(UnixMillisecondsDateTimeOffset left, UnixMillisecondsDateTimeOffset right)
        {
            return left._dateTimeOffset > right._dateTimeOffset;
        }

        public static bool operator <(UnixMillisecondsDateTimeOffset left, UnixMillisecondsDateTimeOffset right)
        {
            return left._dateTimeOffset < right._dateTimeOffset;
        }

        public static bool operator >=(UnixMillisecondsDateTimeOffset left, UnixMillisecondsDateTimeOffset right)
        {
            return left._dateTimeOffset >= right._dateTimeOffset;
        }

        public static bool operator <=(UnixMillisecondsDateTimeOffset left, UnixMillisecondsDateTimeOffset right)
        {
            return left._dateTimeOffset <= right._dateTimeOffset;
        }
    }
}
