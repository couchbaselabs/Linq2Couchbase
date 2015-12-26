using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Parsing;

namespace Couchbase.Linq.Views.QueryGeneration
{
    public class ViewExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotImplementedException();
        }
    }
}
