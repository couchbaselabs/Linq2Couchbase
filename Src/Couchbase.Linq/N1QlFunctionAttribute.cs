using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    /// <summary>
    /// Decorates a method that is converted to a N1QL function call.  Each parameter in .Net
    /// is expected to correspond directly to a parameter in N1QL, in the same order.
    /// </summary>
    /// <remarks>
    /// Should only be used to decorate static methods.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    internal class N1QlFunctionAttribute : Attribute
    {
        /// <summary>
        /// Name of the function in N1QL.
        /// </summary>
        public string N1QlFunctionName { get; set; }

        /// <summary>
        /// Creates a new N1QlFunctionAttribute.
        /// </summary>
        /// <param name="n1QlFunctionName">Name of the function in N1QL.</param>
        public N1QlFunctionAttribute(string n1QlFunctionName)
        {
            if (string.IsNullOrEmpty(n1QlFunctionName))
            {
                throw new ArgumentNullException("n1QlFunctionName");
            }

            N1QlFunctionName = n1QlFunctionName;
        }
    }
}
