using FluentAssertions;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDataValueTests
{
    public class Reset
    {
        [Fact]
        public void ShouldResetToDefault()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.SerializeObject(10, typeof(int)).Returns(10);
            transcoder.EncodeElement(10).Returns(10);

            var sut = CreateValue(owner, serializer);
            sut.Initialize(10, typeof(int));

            sut.Value = 20;
            sut.Reset();

            sut.Value.Should().Be(10);
        }
    }
    
    public class DecodeFailure
    {
        [Fact]
        public void ShouldOverwriteEncodedValueOnDecodeFailure()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.GetSerializedType(typeof(int)).Returns(typeof(int));
            serializer.SerializeObject(10, typeof(int)).Returns(10);
            transcoder.DecodeElement(999, typeof(int)).Returns((object?)null);
            transcoder.EncodeElement(10).Returns(10);

            var sut = CreateValue(owner, serializer);
            sut.EncodedValue = 999;

            sut.Initialize(10, typeof(int));

            sut.EncodedValue.Should().Be(10);
        }
    }
    
    public class Decoding
    {
        [Fact]
        public void ShouldDecodeOnInitialize()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.GetSerializedType(typeof(int)).Returns(typeof(int));
            transcoder.DecodeElement(50, typeof(int)).Returns(50);
            serializer.DeserializeObject(50, typeof(int), null).Returns(50);

            var sut = CreateValue(owner, serializer);
            sut.EncodedValue = 50;

            sut.Initialize(0, typeof(int));

            sut.Value.Should().Be(50);
        }

        [Fact]
        public void ShouldNotDecodeAfterInitialization()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            var sut = CreateValue(owner, serializer);
            sut.Initialize(10, typeof(int));

            sut.EncodedValue = 99;

            sut.Value.Should().Be(10);
        }
    }
    
    public class TranscoderChanged
    {
        [Fact]
        public void ShouldNotHaveEffectWhenUninitialized()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();
            owner.Transcoder.Returns(transcoder);

            var sut = CreateValue(owner, serializer);

            owner.TranscoderChanged += Raise.Event<EventHandler>(owner, EventArgs.Empty);

            Assert.Throws<ValueNotInitializedException>(() => sut.Value);
        }

        [Fact]
        public void ShouldReencodeWhenTranscoderChanges()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.SerializeObject(25, typeof(int)).Returns(25);
            transcoder.EncodeElement(25).Returns(25);

            var sut = CreateValue(owner, serializer);
            sut.Initialize(10, typeof(int));

            sut.Value = 25;

            owner.TranscoderChanged += Raise.Event<EventHandler>(owner, EventArgs.Empty);

            transcoder.Received().EncodeElement(25);
        }
    }
    
    public class Encoding
    {
        [Fact]
        public void ShouldEncodeValueWhenSet()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.SerializeObject(20, typeof(int)).Returns(20);
            transcoder.EncodeElement(20).Returns(20);

            var sut = CreateValue(owner, serializer);
            sut.Initialize(10, typeof(int));

            sut.Value = 20;

            sut.EncodedValue.Should().Be(20);

            transcoder.Received(1).EncodeElement(20);
        }

        [Fact]
        public void ShouldNotEncodeWithoutTranscoder()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            owner.Transcoder.Returns((IPersistentDataTranscoder?)null);

            var serializer = Substitute.For<ISerializer>();

            var sut = CreateValue(owner, serializer);
            sut.Initialize(10, typeof(int));

            sut.Value = 20;

            serializer.DidNotReceive().SerializeObject(Arg.Any<object>(), Arg.Any<Type>());
        }
    }
    
    public class Initialize
    {
        [Fact]
        public void ShouldUseDefaultValueWhenNoEncodedValue()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var sut = CreateValue(owner);

            sut.Initialize(10);

            sut.Value.Should().Be(10);
        }

        [Fact]
        public void ShouldUseEncodedValueIfPresent()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var serializer = Substitute.For<ISerializer>();

            owner.Transcoder.Returns(transcoder);

            serializer.GetSerializedType(typeof(int)).Returns(typeof(int));
            transcoder.DecodeElement(42, typeof(int)).Returns(42);
            serializer.DeserializeObject(42, typeof(int), null).Returns(42);

            var sut = CreateValue(owner, serializer);
            sut.EncodedValue = 42;

            sut.Initialize(0);

            sut.Value.Should().Be(42);
        }

        [Fact]
        public void ShouldThrowIfInitializedTwice()
        {
            var sut = CreateValue(Substitute.For<IPersistentDataValueOwner>());

            sut.Initialize(1);

            Assert.Throws<InvalidOperationException>(() => sut.Initialize(2));
        }
    }
    
    private static PersistentDataValue CreateValue(
        IPersistentDataValueOwner owner,
        ISerializer? serializer = null,
        ILogger<PersistentDataValue>? logger = null)
    {
        serializer ??= Substitute.For<ISerializer>();
        logger ??= Substitute.For<ILogger<PersistentDataValue>>();

        return new PersistentDataValue(owner, serializer, Substitute.For<IExceptionHandler>(), logger)
        {
            Name = "Test"
        };
    }
}