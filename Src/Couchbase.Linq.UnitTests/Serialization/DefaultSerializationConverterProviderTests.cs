using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core.Serialization;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.Serialization;
using Couchbase.Linq.Serialization.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Serialization
{
    [TestFixture]
    public class DefaultSerializationConverterProviderTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DefaultSerializationConverterProvider.Registry = new TypeBasedSerializationConverterRegistry
            {
                { typeof(TestConverter), typeof(TestSerializationConverter)}
            };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DefaultSerializationConverterProvider.Registry =
                TypeBasedSerializationConverterRegistry.CreateDefaultRegistry();
        }

        [Test]
        public void GetSerializationConverter_AppliedToClass_ReturnsConverter()
        {
            // Arrange

            var provider = new DefaultSerializationConverterProvider(new DefaultSerializer());

            var member = typeof(ConverterOnSecondaryClass).GetProperty(nameof(ConverterOnSecondaryClass.Secondary));

            // Act

            var serializationConverter = provider.GetSerializationConverter(member);

            // Assert

            Assert.IsAssignableFrom<TestSerializationConverter>(serializationConverter);
        }

        [Test]
        public void GetSerializationConverter_AppliedToProperty_ReturnsConverter()
        {
            // Arrange

            var provider = new DefaultSerializationConverterProvider(new DefaultSerializer());

            var member = typeof(ConverterOnProperty).GetProperty(nameof(ConverterOnSecondaryClass.Secondary));

            // Act

            var serializationConverter = provider.GetSerializationConverter(member);

            // Assert

            Assert.IsAssignableFrom<TestSerializationConverter>(serializationConverter);
        }

        [Test]
        public void GetSerializationConverter_AppliedToSerializer_ReturnsConverter()
        {
            // Arrange

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new TestConverter()
                }
            };

            var provider = new DefaultSerializationConverterProvider(new DefaultSerializer(settings, settings));

            var member = typeof(NoConverter).GetProperty(nameof(ConverterOnSecondaryClass.Secondary));

            // Act

            var serializationConverter = provider.GetSerializationConverter(member);

            // Assert

            Assert.IsAssignableFrom<TestSerializationConverter>(serializationConverter);
        }

        #region Helpers

        public class ConverterOnSecondaryClass
        {
            public SecondaryClassWithConverter Secondary { get; set; }
        }

        public class ConverterOnProperty
        {
            [JsonConverter(typeof(TestConverter))]
            public SecondaryClassWithoutConverter Secondary { get; set; }
        }

        public class NoConverter
        {
            public SecondaryClassWithoutConverter Secondary { get; set; }
        }

        public class SecondaryClassWithoutConverter
        {
        }

        [JsonConverter(typeof(TestConverter))]
        public class SecondaryClassWithConverter
        {
        }

        public class TestConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(SecondaryClassWithConverter) ||
                       objectType == typeof(SecondaryClassWithoutConverter);
            }
        }

        public class TestSerializationConverter : SerializationConverterBase, ISerializationConverter<string>
        {
            protected override IDictionary<Type, MethodInfo> ConvertFromMethods =>
                GetConvertFromMethods<TestSerializationConverter>();

            protected override IDictionary<Type, MethodInfo> ConvertToMethods  =>
                GetConvertToMethods<TestSerializationConverter>();

            protected override void RenderConvertToMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
            {
                throw new NotImplementedException();
            }

            protected override void RenderConvertFromMethod(Expression innerExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
            {
                throw new NotImplementedException();
            }

            protected override void RenderConvertedConstant(ConstantExpression constantExpression, IN1QlExpressionTreeVisitor expressionTreeVisitor)
            {
                throw new NotImplementedException();
            }

            public string ConvertTo(string value)
            {
                throw new NotImplementedException();
            }

            public string ConvertFrom(string value)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
