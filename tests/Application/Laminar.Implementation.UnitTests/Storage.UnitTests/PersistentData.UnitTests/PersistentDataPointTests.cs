using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDataPointTests
{
    public class Reset
    {
        [Fact]
        public void ShouldForgetMaterializedValue()
        {
            var value = Substitute.For<IPersistentValue<string>>();
            var valueFactory = Substitute.For<IEncodableDataFactory>();
            valueFactory.GetValueWithDefault("default", null, null).Returns(value);

            var sut = new PersistentDataPoint(valueFactory);

            sut.GetValueOrDefault("default");

            sut.Reset();

            var replacement = Substitute.For<IPersistentValue<string>>();
            valueFactory.GetValueWithDefault("default", null, null)
                .Returns(replacement);

            sut.GetValueOrDefault("default")
                .Should()
                .BeSameAs(replacement);
        }

        [Fact]
        public void ShouldClearEncodedValue()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var sut = new PersistentDataPoint(Substitute.For<IEncodableDataFactory>());

            sut.Decode(transcoder, "encoded");
            sut.Reset();

            var action = () => sut.Encode(transcoder);

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*Cannot encode uninitialized data point*");
        }
    }
    
    public class GetOrCreateCollection
    {
        [Fact]
        public void ShouldReturnKnownValueWhenProvided()
        {
            var collection = Substitute.For<IEncodablePersistentData>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetEncodableData<IEncodablePersistentData>().Returns(collection);

            var sut = new PersistentDataPoint(dataFactory);

            var result = sut.GetOrCreateCollection(collection);

            result.Should().BeSameAs(collection);
        }

        [Fact]
        public void ShouldResolveCollectionFromServiceProviderWhenKnownValueNull()
        {
            var collection = Substitute.For<IEncodablePersistentData>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetEncodableData<IEncodablePersistentData>().Returns(collection);

            var sut = new PersistentDataPoint(dataFactory);
            
            var result = sut.GetOrCreateCollection<IEncodablePersistentData>(null);

            result.Should().BeSameAs(collection);
        }

        [Fact]
        public void ShouldReturnExistingCollection()
        {
            var collection = Substitute.For<IEncodablePersistentData>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetEncodableData<IEncodablePersistentData>().Returns(collection);

            var sut = new PersistentDataPoint(dataFactory);

            var result1 = sut.GetOrCreateCollection(collection);
            var result2 = sut.GetOrCreateCollection<IEncodablePersistentData>(null);

            result2.Should().BeSameAs(result1);
        }

        [Fact]
        public void ShouldDecodeWhenEncodedValueExists()
        {
            var collection = Substitute.For<IEncodablePersistentData>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            dataFactory.GetEncodableData<IEncodablePersistentData>().Returns(collection);

            var sut = new PersistentDataPoint(dataFactory);

            sut.Decode(transcoder, "encoded");

            sut.GetOrCreateCollection(collection);

            collection.Received(1).Decode(transcoder, "encoded");
        }

        [Fact]
        public void ShouldSubscribeToInvalidation()
        {
            var collection = Substitute.For<IEncodablePersistentData>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetEncodableData<IEncodablePersistentData>().Returns(collection);

            var sut = new PersistentDataPoint(dataFactory);

            sut.GetOrCreateCollection(collection);

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            collection.OnInvalidated += Raise.Event<EventHandler>(collection, EventArgs.Empty);

            raised.Should().BeTrue();
        }
    }
    
    public class GetValue
    {
        [Fact]
        public void ShouldThrowWhenDataPointHasNoEncodedValue()
        {
            var sut = new PersistentDataPoint(Substitute.For<IEncodableDataFactory>());

            var action = () => sut.GetValue<string>();

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*needs both a transcoder and encoded value*");
        }

        [Fact]
        public void ShouldCreateValueFromEncodedData()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var value = Substitute.For<IPersistentValue<string>>();
            var factory = Substitute.For<IEncodableDataFactory>();

            factory.GetValueFromEncoded<string>("encoded", transcoder, null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.Decode(transcoder, "encoded");
            var result = sut.GetValue<string>();
            result.Should().BeSameAs(value);
        }

        [Fact]
        public void ShouldOnlyMaterializeOnce()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var value = Substitute.For<IPersistentValue<string>>();
            var factory = Substitute.For<IEncodableDataFactory>();

            factory.GetValueFromEncoded<string>(
                    Arg.Any<object>(),
                    Arg.Any<IPersistentDataTranscoder>(),
                    Arg.Any<Type>(),
                    Arg.Any<object>())
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.Decode(transcoder, "encoded");

            var result1 = sut.GetValue<string>();
            var result2 = sut.GetValue<string>();

            result2.Should().BeSameAs(result1);

            factory.Received(1)
                .GetValueFromEncoded<string>(
                    Arg.Any<object>(),
                    Arg.Any<IPersistentDataTranscoder>(),
                    Arg.Any<Type>(),
                    Arg.Any<object>());
        }

        [Fact]
        public void ShouldSubscribeToInvalidation()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var value = Substitute.For<IPersistentValue<string>>();
            var factory = Substitute.For<IEncodableDataFactory>();

            factory.GetValueFromEncoded<string>(
                    Arg.Any<object>(),
                    Arg.Any<IPersistentDataTranscoder>(),
                    Arg.Any<Type>(),
                    Arg.Any<object>())
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.Decode(transcoder, "encoded");

            sut.GetValue<string>();

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            value.OnInvalidated += Raise.Event<EventHandler>(value, EventArgs.Empty);

            raised.Should().BeTrue();
        }
    }
    
    public class GetValueOrDefault
    {
        [Fact]
        public void ShouldCreateValueUsingFactory()
        {
            var value = Substitute.For<IPersistentValue<string>>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            var result = sut.GetValueOrDefault("default");

            result.Should().BeSameAs(value);
        }

        [Fact]
        public void ShouldDecodeExistingEncodedValue()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var value = Substitute.For<IPersistentValue<string>>();
            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.Decode(transcoder, "encoded");

            sut.GetValueOrDefault("default");

            value.Received(1)
                .Decode(transcoder, "encoded");
        }

        [Fact]
        public void ShouldNotDecodeWhenNoEncodedValueExists()
        {
            var value = Substitute.For<IPersistentValue<string>>();
            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.GetValueOrDefault("default");

            value.DidNotReceive()
                .Decode(
                    Arg.Any<IPersistentDataTranscoder>(),
                    Arg.Any<object>());
        }
    }
    
    public class Encode
    {
        [Fact]
        public void ShouldThrowWhenUninitialized()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var sut = new PersistentDataPoint(Substitute.For<IEncodableDataFactory>());

            var action = () => sut.Encode(transcoder);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldReturnStoredEncodedValueWhenNotMaterialized()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var sut = new PersistentDataPoint(Substitute.For<IEncodableDataFactory>());
            sut.Decode(transcoder, "encoded");

            var result = sut.Encode(transcoder);

            result.Should().Be("encoded");
        }

        [Fact]
        public void ShouldEncodeMaterializedValue()
        {
            var value = Substitute.For<IPersistentValue<string>>();
            value.Encode(Arg.Any<IPersistentDataTranscoder>())
                .Returns("encoded");
            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.GetValueOrDefault("default");

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var result = sut.Encode(transcoder);

            result.Should().Be("encoded");
        }

        [Fact]
        public void ShouldReuseCachedEncodingForSameTranscoder()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var value = Substitute.For<IPersistentValue<string>>();
            value.Encode(transcoder).Returns("encoded");

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.GetValueOrDefault("default");

            sut.Encode(transcoder);
            sut.Encode(transcoder);

            value.Received(1).Encode(transcoder);
        }

        [Fact]
        public void ShouldReencodeForDifferentTranscoder()
        {
            var transcoder1 = Substitute.For<IPersistentDataTranscoder>();
            var transcoder2 = Substitute.For<IPersistentDataTranscoder>();

            var value = Substitute.For<IPersistentValue<string>>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.GetValueOrDefault("default");

            sut.Encode(transcoder1);
            sut.Encode(transcoder2);

            value.Received(1).Encode(transcoder1);
            value.Received(1).Encode(transcoder2);
        }
    }
    
    public class Decode
    {
        [Fact]
        public void ShouldStoreEncodedValue()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var sut = new PersistentDataPoint(Substitute.For<IEncodableDataFactory>());

            sut.Decode(transcoder, "encoded");

            sut.Encode(transcoder).Should().Be("encoded");
        }

        [Fact]
        public void ShouldForwardDecodeToMaterializedValue()
        {
            var value = Substitute.For<IPersistentValue<string>>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.GetValueOrDefault("default");

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            sut.Decode(transcoder, "encoded");

            value.Received(1).Decode(transcoder, "encoded");
        }

        [Fact]
        public void ShouldNotDecodeWhenEncodedReferenceHasNotChanged()
        {
            var value = Substitute.For<IPersistentValue<string>>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetValueWithDefault("default", null, null)
                .Returns(value);

            var sut = new PersistentDataPoint(factory);

            sut.GetValueOrDefault("default");

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var encoded = new object();

            sut.Decode(transcoder, encoded);
            sut.Decode(transcoder, encoded);

            value.Received(1).Decode(transcoder, encoded);
        }
    }
}