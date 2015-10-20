using System;
using Couchbase.IO;

namespace Couchbase.Linq
{
    /// <summary>
    /// Thrown if a write operation fails - use the InnerException, Message and ResponseStatus properties to determine what went wrong.
    /// </summary>
    public class CouchbaseWriteException : Exception
    {
        private readonly IOperationResult _failedResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="CouchbaseWriteException"/> class.
        /// </summary>
        /// <param name="failedResult">The <see cref="IOperationResult"/> of the failed operation.</param>
        public CouchbaseWriteException(IOperationResult failedResult)
            : base(failedResult.Message, failedResult.Exception)
        {
            _failedResult = failedResult;
        }

        /// <summary>
        /// Gets the failure <see cref="ResponseStatus"/>.
        /// </summary>
        /// <value>
        /// The response status.
        /// </value>
        public ResponseStatus ResponseStatus
        {
            get { return _failedResult.Status; }
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2015 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
