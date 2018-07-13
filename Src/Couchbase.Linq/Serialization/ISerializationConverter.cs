using System;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// A converter which represents conversions that normally take place during JSON serialization.
    /// </summary>
    public interface ISerializationConverter
    {
        /// <summary>
        /// Wraps an expression to replicate the conversion which is happening during serialization.
        /// </summary>
        /// <param name="innerExpression">Expression to be converted.</param>
        /// <returns>A new expression.</returns>
        /// <remarks>
        /// The in code representation of the conversion should be a noop.  It acts as a placeholder for query
        /// generation.  The <see cref="RenderConvertTo"/> method will be called to render the N1QL equivalent
        /// when the <see cref="ISerializationConverter{T}.ConvertTo"/> method is encountered.
        /// </remarks>
        Expression GenerateConvertToExpression(Expression innerExpression);

        /// <summary>
        /// Wraps an expression to replicate the inverse of the conversion which is happening during serialization.
        /// </summary>
        /// <param name="innerExpression">Expression to be converted.</param>
        /// <returns>A new expression.</returns>
        /// <remarks>
        /// The in code representation of the conversion should be a noop.  It acts as a placeholder for query
        /// generation.  The <see cref="RenderConvertTo"/> method will be called to render the N1QL equivalent
        /// when the <see cref="ISerializationConverter{T}.ConvertFrom"/> method is encountered.
        /// </remarks>
        Expression GenerateConvertFromExpression(Expression innerExpression);

        /// <summary>
        /// Renders the conversion to a query string builder.
        /// </summary>
        /// <param name="innerExpression">Inner expression being converted.</param>
        /// <param name="expressionTreeVisitor"><see cref="IN1QlExpressionTreeVisitor"/> used for rendering.</param>
        void RenderConvertTo(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor);

        /// <summary>
        /// Renders the inverse conversion to a query string builder.
        /// </summary>
        /// <param name="innerExpression">Inner expression being converted.</param>
        /// <param name="expressionTreeVisitor"><see cref="IN1QlExpressionTreeVisitor"/> used for rendering.</param>
        void RenderConvertFrom(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor);
    }
}
