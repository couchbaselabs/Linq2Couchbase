using System;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Serialization;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Couchbase.Linq.Utils
{
    /// <summary>
    /// Implementation of <see cref="IEvaluatableExpressionFilter"/> which prevents
    /// pre-evaluation of calls to <see cref="ISerializationConverter{T}"/> methods.
    /// </summary>
    internal sealed class ExcludeSerializationConversionEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
        public override bool IsEvaluatableMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType != null)
            {
                var typeInfo = node.Method.DeclaringType.GetTypeInfo();
                if (typeInfo.IsInterface && typeInfo.IsGenericType)
                {
                    return typeInfo.GetGenericTypeDefinition() != typeof(ISerializationConverter<>);
                }
            }

            return true;
        }
    }
}
