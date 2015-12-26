using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration.MethodCallTranslators
{
    internal class MathMethodCallTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Maps System.Math method call names to N1QL functions
        /// </summary>
        private static readonly Dictionary<String, String> SupportedMethodNames =
            new Dictionary<String, String> 
        {
            {"Abs", "ABS"},
            {"Acos", "ACOS"},
            {"Atan", "ATAN"},
            {"Atan2", "ATAN2"},
            {"Asin", "ASIN"},
            {"Ceiling", "CEIL"},
            {"Cos", "COS"},
            {"Exp", "EXP"},
            {"Floor", "FLOOR"},
            {"Log", "LN"},
            {"Log10", "LOG"},
            {"Pow", "POWER"},
            {"Round", "ROUND"},
            {"Sign", "SIGN"},
            {"Sin", "SIN"},
            {"Sqrt", "SQRT"},
            {"Tan", "TAN"},
            {"Truncate", "TRUNC"}
        };

        /// <summary>
        /// Returns all numeric types for parameters of System.Math method overloads
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetNumericTypes()
        {
            yield return typeof (byte);
            yield return typeof (sbyte);
            yield return typeof (short);
            yield return typeof (ushort);
            yield return typeof (int);
            yield return typeof (uint);
            yield return typeof (long);
            yield return typeof (ulong);
            yield return typeof (decimal);
            yield return typeof (float);
            yield return typeof (double);
        }

        /// <summary>
        /// Given a parameter type, get all supported methods on System.Math that accept this parameter type
        /// </summary>
        private static IEnumerable<MethodInfo> GetMathMethodsForType(Type t)
        {
            // Find methods which are in the SupportedMethodNames dictionary
            // And where all parameter types match t

            var methods = typeof (Math).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => SupportedMethodNames.Keys.Contains(method.Name));

            foreach (MethodInfo method in methods.Where(method => method.GetParameters().All(parameter => parameter.ParameterType == t)))
            {
                yield return method;
            }

            if ((t == typeof (decimal)) || (t == typeof (double)))
            {
                // Also need to pickup Math.Round with a single integer second parameter
                yield return typeof (Math).GetMethod("Round", new Type[] {t, typeof (int)});
            }
        } 

        private static readonly MethodInfo[] SupportedMethodsStatic =
            GetNumericTypes().SelectMany(GetMathMethodsForType).ToArray();
            
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

            string functionName;
            if (!SupportedMethodNames.TryGetValue(methodCallExpression.Method.Name, out functionName))
            {
                throw new NotSupportedException("Unsupported Math Method");
            }

            expression.AppendFormat("{0}(", functionName);

            for (var i=0; i<methodCallExpression.Arguments.Count; i++)
            {
                if (i > 0)
                {
                    expression.Append(", ");
                }

                expressionTreeVisitor.Visit(methodCallExpression.Arguments[i]);
            }
            
            expression.Append(')');

            return methodCallExpression;
        }
    }
}
