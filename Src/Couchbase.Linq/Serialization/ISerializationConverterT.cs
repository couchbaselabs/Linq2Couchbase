using System;

namespace Couchbase.Linq.Serialization
{
    /// <inheritdoc/>
    public interface ISerializationConverter<T> : ISerializationConverter
    {
        /// <summary>
        /// Noop method call, used as a placeholder for a conversion replicating the
        /// one being performed during serialization.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The value which was converted.</returns>
        T ConvertTo(T value);

        /// <summary>
        /// Noop method call, used as a placeholder replicating the inverse of the conversion
        /// being performed during serialization.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The value which was converted.</returns>
        T ConvertFrom(T value);
    }
}
