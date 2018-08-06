# Serialization Converters

## Overview

Some attributes may have additional decorators applied that change how they are serialized, and this can alter the behavior required when accessing these properties in queries.  The rendered N1QL query needs to account for any differences in serialization for comparisons and more.

As an example, by default Linq2Couchbase assumes that DateTime properties are serialized as ISO 8601 strings.  When using the default serializer, `JsonConverter` attributes may be used to indicate that they are stored as milliseconds since the Unix epoch.  See [Date Handling](./date-handling.md) for more information.  In this case, conversion from string to Unix milliseconds before performing comparisons is no longer required.

### Built-in Converters

There is built-in support for the following Newtonsoft.Json converters:

- UnixMillisecondsConverter

## Adding Support for Custom Converters

### Writing A Custom Serialization Converter

To add support for additional `JsonConverter` implementations, implement a class which inherits from `ISerializerConverter<T>`.  The same class may cover multiple types by implementing the interface multiple times.  Additionally, a lot of common implementation details can be provided by inheriting from `SerializationConverterBase`.

The method implementations for `ISerializerConverter<T>` are just placeholders within the LINQ expression tree, so they should just act as a noop.

```cs
DateTime ISerializationConverter<DateTime>.ConvertTo(DateTime value)
{
    return value;
}

DateTime ISerializationConverter<DateTime>.ConvertFrom(DateTime value)
{
    return value;
}
```

Implementing `ConvertFromMethods` and `ConvertToMethods` is easiest using the helper static methods, which use reflection to find all implementations of `ISerializerConverter<T>` on your class.  Be sure to store these statically for performance.

```cs
private static readonly IDictionary<Type, MethodInfo> ConvertFromMethodsStatic =
    GetConvertFromMethods<UnixMillisecondsSerializationConverter>();

private static readonly IDictionary<Type, MethodInfo> ConvertToMethodsStatic =
    GetConvertToMethods<UnixMillisecondsSerializationConverter>();

protected override IDictionary<Type, MethodInfo> ConvertFromMethods => ConvertFromMethodsStatic;
protected override IDictionary<Type, MethodInfo> ConvertToMethods => ConvertToMethodsStatic;
```

`RenderConvertToMethod` and `RenderConvertFromMethod` render conversion logic onto the N1QL query.  `RenderConvertToMethod` should convert from the standard serialized format to the format used by the customer converter.  `RenderConvertFromMethod` should do the inverse, converting from the custom format back to the standard format.  Note that inverting calls (converting one way and then back) are automatically excluded, and these methods will be skipped.

```cs
protected override void RenderConvertToMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
{
    expressionTreeVisitor.Expression.Append("STR_TO_MILLIS(");
    expressionTreeVisitor.Visit(innerExpression);
    expressionTreeVisitor.Expression.Append(')');
}

protected override void RenderConvertFromMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
{
    expressionTreeVisitor.Expression.Append("MILLIS_TO_STR(");
    expressionTreeVisitor.Visit(innerExpression);
    expressionTreeVisitor.Expression.Append(')');
}
```

Finally, `RenderConvertedConstant` is used to render a constant value which has already been converted.  This is typically used when comparing a constant value to a property with a customer JSON converter.

```cs
protected override void RenderConvertedConstant(ConstantExpression constantExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
{
    if (constantExpression.Value == null)
    {
        // Don't try to convert nulls
        expressionTreeVisitor.Visit(constantExpression);
    }
    else
    {
        var dateTime = GetDateTime(constantExpression);
        var unixMilliseconds = (dateTime - UnixEpoch).TotalMilliseconds;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Math.Floor(unixMilliseconds) == unixMilliseconds)
        {
            expressionTreeVisitor.Expression.Append((long) unixMilliseconds);
        }
        else
        {
            expressionTreeVisitor.Expression.Append(unixMilliseconds);
        }
    }
}

private static DateTime GetDateTime(ConstantExpression constantExpression)
{
    switch (constantExpression.Value)
    {
        case DateTime dateTime:
            if (dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }
            return dateTime;

        case DateTimeOffset offset:
            return offset.UtcDateTime;

        default:
            throw new InvalidOperationException("ConstantExpression is not a DateTime or equivalent");
    }
}
```

### Registering Custom Serialization Converters

For consumers using Json.Net, you can easily register a custom converter globally, using `TypeBasedSerializationConverterRegistry.Global`.  This registry is used by default by `DefaultSerializationConverterProvider`, unless overridden.  It is preloaded with the built-in converters.

```cs
TypeBasedSerializationConverterRegistry.Global.Add(typeof(MyJsonConverter), typeof(MyConverterSerializer));
```

To provide a more advanced registry, you can replace `DefaultSerializationConverterProvider.Registry` with a completely different `IJsonNetSerializationConverterRegistry`.

## Custom Serialization Libraries

`ISerializationConverter<T>` implementations can also be used with custom serialization libraries.  See [Custom Serializers](./custom-serializers.md) for more information.