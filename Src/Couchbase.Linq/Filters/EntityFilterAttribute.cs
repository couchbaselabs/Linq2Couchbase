using System;

namespace Couchbase.Linq.Filters
{
    /// <summary>
    /// Abstract base class for attribute-based <see cref="IEntityFilter&lt;T&gt;">IEntityFilter</see> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class EntityFilterAttribute : Attribute
    {
        /// <summary>
        /// Priority of this filter compared to other filters against the same type.  Lower priorities execute first.
        /// </summary>
        public int Priority { get; set; }

        public abstract IEntityFilter<T> GetFilter<T>(); 

    }
}
