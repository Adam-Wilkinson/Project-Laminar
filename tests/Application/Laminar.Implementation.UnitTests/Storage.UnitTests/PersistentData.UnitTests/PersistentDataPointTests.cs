using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDataPointTests
{
    public class Reset
    {
        [Fact]
        public void ShouldResetToDefault()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.SerializeObject(10, typeof(int)).Returns(10);
            transcoder.EncodeElement(10).Returns(10);

            var sut = CreateValue(owner, serializer);
            sut.GetValueOrDefault(10, typeof(int));

            sut.GetValue<int>().Value = 20;
            sut.GetValue<int>().Reset();

            sut.GetValue<int>().Value.Should().Be(10);
        }
    }
    
    public class DecodeFailure
    {
        [Fact]
        public void ShouldOverwriteEncodedValueOnDecodeFailure()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.GetSerializedType(typeof(int)).Returns(typeof(int));
            serializer.SerializeObject(10, typeof(int)).Returns(10);
            transcoder.DecodeElement(999, typeof(int)).Returns((object?)null);
            transcoder.EncodeElement(10).Returns(10);

            var sut = CreateValue(owner, serializer);
            sut.EncodedValue = 999;

            sut.GetValueOrDefault(10, typeof(int));

            sut.EncodedValue.Should().Be(10);
        }
    }
    
    public class Decoding
    {
        [Fact]
        public void ShouldDecodeOnInitialize()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.GetSerializedType(typeof(int)).Returns(typeof(int));
            transcoder.DecodeElement(50, typeof(int)).Returns(50);
            serializer.DeserializeObject(new DeserializationRequest
            {
                Serialized = 50,
                TargetType = typeof(int),
                Context = null,
                ExistingInstance = 0,
            }).Returns(50);

            var sut = CreateValue(owner, serializer);
            sut.EncodedValue = 50;

            sut.GetValueOrDefault(0, typeof(int));

            sut.GetValue<int>().Value.Should().Be(50);
        }

        [Fact]
        public void ShouldNotDecodeAfterInitialization()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            var sut = CreateValue(owner, serializer);
            sut.GetValueOrDefault(10, typeof(int));

            sut.EncodedValue = 99;

            sut.GetValue<int>().Value.Should().Be(10);
        }
    }
    
    public class TranscoderChanged
    {
        [Fact]
        public void ShouldNotHaveEffectWhenUninitialized()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();
            owner.Transcoder.Returns(transcoder);

            var sut = CreateValue(owner, serializer);

            owner.TranscoderChanged += Raise.Event<EventHandler>(owner, EventArgs.Empty);

            Assert.Throws<InvalidOperationException>(sut.GetValue<int>);
        }

        [Fact]
        public void ShouldReEncodeWhenTranscoderChanges()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.SerializeObject(25, typeof(int)).Returns(25);
            transcoder.EncodeElement(25).Returns(25);

            var sut = CreateValue(owner, serializer);
            sut.GetValueOrDefault(10, typeof(int));

            sut.GetValue<int>().Value = 25;

            owner.TranscoderChanged += Raise.Event<EventHandler>(owner, EventArgs.Empty);

            var newEncodedValue = sut.EncodedValue;
            transcoder.Received().EncodeElement(25);
        }
    }
    
    public class Encoding
    {
        [Fact]
        public void ShouldEncodeValueWhenSet()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.SerializeObject(20, typeof(int)).Returns(20);
            transcoder.EncodeElement(20).Returns(20);

            var sut = CreateValue(owner, serializer);
            sut.GetValueOrDefault(10, typeof(int));

            sut.GetValue<int>().Value = 20;

            sut.EncodedValue.Should().Be(20);

            transcoder.Received(1).EncodeElement(20);
        }

        [Fact]
        public void ShouldNotEncodeWithoutTranscoder()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            owner.Transcoder.Returns((IPersistentDataTranscoder?)null);

            var serializer = Substitute.For<ISerializer>();

            var sut = CreateValue(owner, serializer);
            sut.GetValueOrDefault(10, typeof(int));

            sut.GetValue<int>().Value = 20;

            serializer.DidNotReceive().SerializeObject(Arg.Any<object>(), Arg.Any<Type>());
        }
    }
    
    public class Initialize
    {
        [Fact]
        public void ShouldUseDefaultValueWhenNoEncodedValue()
        {
            var sut = CreateValue();

            sut.GetValueOrDefault(10);

            sut.GetValue<int>().Value.Should().Be(10);
        }

        [Fact]
        public void ShouldUseEncodedValueIfPresent()
        {
            var owner = Substitute.For<IPersistentDataNode>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            transcoder.DecodeElement(42, typeof(int)).Returns(42);
            serializer.GetSerializedType(typeof(int)).Returns(typeof(int));
            serializer.DeserializeObject(new DeserializationRequest
            {
                Serialized = 42,
                TargetType = typeof(int),
                Context = null,
                ExistingInstance = 0,
            }).Returns(42);

            var sut = CreateValue(owner, serializer);
            
            sut.EncodedValue = 42;
            sut.GetValueOrDefault(0);

            sut.GetValue<int>().Value.Should().Be(42);
        }

        [Fact]
        public void ShouldThrowIfInitializedTwice()
        {
            var sut = CreateValue();

            sut.GetValueOrDefault(1);

            Assert.Throws<InvalidOperationException>(() => sut.GetValueOrDefault(2));
        }
    }
    
    private static PersistentDataPoint CreateValue(
        IPersistentDataNode? owner = null,
        ISerializer? serializer = null,
        ILogger<PersistentDataPoint>? logger = null,
        IExceptionHandler? exceptionHandler = null)
    {
        owner ??= Substitute.For<IPersistentDataNode>();
        serializer ??= Substitute.For<ISerializer>();
        logger ??= Substitute.For<ILogger<PersistentDataPoint>>();
        exceptionHandler ??= Substitute.For<IExceptionHandler>();

        return new PersistentDataPoint(owner, serializer, exceptionHandler, logger);
    }
}