namespace Couchbase.Linq
{
    public static partial class N1QlFunctions
    {
        #region IsMissing and IsNotMissing

        /// <summary>
        /// Returns true if the selected property is missing from the document
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is missing from the document</returns>
        public static bool IsMissing<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Always returns false

            return false;
        }

        /// <summary>
        /// Returns true if the named property is missing from the document
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is missing from the document</returns>
        /// <remarks>propertyName must be a constant when used in a LINQ expression</remarks>
        public static bool IsMissing<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present via reflection

            return (document == null) || (typeof (T).GetProperty(propertyName) == null);
        }

        /// <summary>
        /// Returns true if the selected property is present on the document
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is present on the document</returns>
        public static bool IsNotMissing<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Always returns true

            return true;
        }

        /// <summary>
        /// Returns true if the named property is present on the document
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is present on the document</returns>
        /// <remarks>propertyName must be a constant when used in a LINQ expression</remarks>
        public static bool IsNotMissing<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present via reflection

            return (document != null) && (typeof(T).GetProperty(propertyName) != null);
        }

        #endregion

        #region IsValued and IsNotValued

        /// <summary>
        /// Returns true if the selected property is present on the document and not null
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is present on the document and not null</returns>
        public static bool IsValued<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Simply test for null

            return property != null;
        }

        /// <summary>
        /// Returns true if the named property is not missing from the document and not null
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is present on the document and not null</returns>
        /// <remarks>propertyName must be a constant when used in a LINQ expression</remarks>
        public static bool IsValued<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present and not null via reflection

            if (document == null)
            {
                return false;
            }

            var property = typeof (T).GetProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            return property.GetValue(document) != null;
        }

        /// <summary>
        /// Returns true if the selected property is missing from the document or null
        /// </summary>
        /// <typeparam name="T">Type of the property being selected</typeparam>
        /// <param name="property">Property to test</param>
        /// <returns>True if the property is missing from the document or null</returns>
        public static bool IsNotValued<T>(T property)
        {
            // Implementation will only be called when unit testing
            // Since properties cannot be missing on in-memory objects
            // Simply test for null

            return property == null;
        }

        /// <summary>
        /// Returns true if the named property is missing from the document or null
        /// </summary>
        /// <typeparam name="T">Type of the document being tested</typeparam>
        /// <param name="document">Document being tested</param>
        /// <param name="propertyName">Property name to test</param>
        /// <returns>True if the property is missing from the document or null</returns>
        /// <remarks>propertyName must be a constant when used in a LINQ expression</remarks>
        public static bool IsNotValued<T>(T document, string propertyName)
        {
            // Implementation will only be called when unit testing
            // Test to see if the property is present via reflection

            if (document == null)
            {
                return true;
            }

            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                return true;
            }

            return property.GetValue(document) == null;
        }

        #endregion
    }
}
