﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class DictionaryKeysMethodCallTranslator  : IMethodCallTranslator
    {
        private static readonly MethodInfo[] SupportedMethodsStatic = {
            typeof (IDictionary).GetMethod("get_Keys")!,
            typeof (IDictionary<,>).GetMethod("get_Keys")!,
            typeof (Dictionary<,>).GetMethod("get_Keys")!
        };

        public IEnumerable<MethodInfo> SupportMethods => SupportedMethodsStatic;

        public Expression Translate(MethodCallExpression methodCallExpression, N1QlExpressionTreeVisitor expressionTreeVisitor)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException(nameof(methodCallExpression));
            }

            var expression = expressionTreeVisitor.Expression;

            expression.Append("OBJECT_NAMES(");
            expressionTreeVisitor.Visit(methodCallExpression.Object!);
            expression.Append(')');

            return methodCallExpression;
        }
    }
}
