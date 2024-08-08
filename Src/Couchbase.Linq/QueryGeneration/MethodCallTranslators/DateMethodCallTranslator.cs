using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators;

internal class DateMethodCallTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo[] SupportedMethodsStatic =
    {
        typeof (DateTime).GetMethod("get_Date"),
        typeof (DateTimeOffset).GetMethod("get_Date")
    };

    public IEnumerable<MethodInfo> SupportMethods
    {
        get
        {
            return SupportedMethodsStatic;
        }
    }

    public Expression Translate(MethodCallExpression methodCallExpression, N1QlExpressionTreeVisitor expressionTreeVisitor)
    {
        if (methodCallExpression == null)
        {
            throw new ArgumentNullException("methodCallExpression");
        }

        var expression = expressionTreeVisitor.Expression;

        expression.Append("DATE_TRUNC_STR(");
        expressionTreeVisitor.Visit(methodCallExpression.Object);
        expression.Append(@",""day"")");

        return methodCallExpression;
    }
}