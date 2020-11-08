using System.Linq.Expressions;

namespace Couchbase.Linq.QueryGeneration
{
    internal interface IMethodCallTranslatorProvider
    {
        IMethodCallTranslator? GetTranslator(MethodCallExpression methodCallExpression);
    }
}
