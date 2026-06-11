using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentValueTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitializeValue()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();
            serializer.GetSerializedValueChangedNotifier("initial", typeof(string)).Returns(notifier);

            var sut = new PersistentValue<string>("initial", null, null, serializer);

            sut.Value.Should().Be("initial");
        }

        [Fact]
        public void ShouldHaveDefaultValue()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();
            serializer.GetSerializedValueChangedNotifier("initial", typeof(string)).Returns(notifier);

            var sut = new PersistentValue<string>("initial", null, null, serializer);

            sut.HasDefaultValue.Should().BeTrue();
            sut.DefaultValue.Should().Be("initial");
        }

        [Fact]
        public void ShouldEstablishValueNotifier()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();
            serializer.GetSerializedValueChangedNotifier("initial", typeof(string)).Returns(notifier);

            _ = new PersistentValue<string>("initial", null, null, serializer);

            serializer.Received(1).GetSerializedValueChangedNotifier("initial", typeof(string));
        }
    }

    public class FromEncodedValue
    {
        [Fact]
        public void ShouldDecodeInitialValue()
        {
            const string encoded = "encoded";
            const string decoded = "decoded";
            const string value = "value";
            
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();
            
            transcoder.DecodeElement(encoded, typeof(string)).Returns(decoded);
            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));
            serializer.DeserializeObject(
                    Arg.Is<DeserializationRequest>(r =>
                        Equals(r.Serialized, decoded) &&
                        r.TargetType == typeof(string)))
                .Returns(value);
            serializer.GetSerializedValueChangedNotifier(value, typeof(string)).Returns(notifier);

            var sut = PersistentValue<string>.FromEncodedValue(
                encoded,
                null,
                null,
                serializer,
                transcoder);

            sut.Value.Should().Be(value);
        }

        [Fact]
        public void ShouldNotHaveDefaultValue()
        {
            const string encoded = "encoded";
            const string decoded = "decoded";
            const string value = "value";
            
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();

            transcoder.DecodeElement(encoded, typeof(string)).Returns(decoded);
            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));
            serializer.DeserializeObject(Arg.Any<DeserializationRequest>()).Returns(value);
            serializer.GetSerializedValueChangedNotifier(value, typeof(string)).Returns(notifier);

            var sut = PersistentValue<string>.FromEncodedValue(
                encoded, null, null, serializer, transcoder);

            sut.HasDefaultValue.Should().BeFalse();
        }

        [Fact]
        public void ShouldThrowWhenDefaultValueRequested()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();

            transcoder.DecodeElement(Arg.Any<object>(), typeof(string)).Returns("decoded");
            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));
            serializer.DeserializeObject(Arg.Any<DeserializationRequest>()).Returns("value");
            serializer.GetSerializedValueChangedNotifier(Arg.Any<object>(), typeof(string)).Returns(notifier);

            var sut = PersistentValue<string>.FromEncodedValue(
                "encoded",
                null,
                null,
                serializer,
                transcoder);

            var action = () => _ = sut.DefaultValue;
            action.Should().Throw<Exception>().WithMessage("*does not have a default*");
        }
    }

    public class Value
    {
        [Fact]
        public void ShouldDisposePreviousNotifierWhenValueChanges()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier1 = Substitute.For<INotifySerializedValueChanged>();
            var notifier2 = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("value1", typeof(string)).Returns(notifier1);

            serializer.GetSerializedValueChangedNotifier("value2", typeof(string)).Returns(notifier2);

            _ = new PersistentValue<string>(
                "value1",
                null,
                null,
                serializer)
            {
                Value = "value2"
            };

            notifier1.Received(1).Dispose();
        }

        [Fact]
        public void ShouldCreateNewNotifierWhenValueChanges()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier1 = Substitute.For<INotifySerializedValueChanged>();
            var notifier2 = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("value1", typeof(string)).Returns(notifier1);
            serializer.GetSerializedValueChangedNotifier("value2", typeof(string)).Returns(notifier2);

            _ = new PersistentValue<string>(
                "value1",
                null,
                null,
                serializer)
            {
                Value = "value2"
            };

            serializer.Received(1).GetSerializedValueChangedNotifier("value2", typeof(string));
        }

        [Fact]
        public void ShouldRaiseInvalidatedWhenValueChanges()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier1 = Substitute.For<INotifySerializedValueChanged>();
            var notifier2 = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("value1", typeof(string))
                .Returns(notifier1);

            serializer.GetSerializedValueChangedNotifier("value2", typeof(string))
                .Returns(notifier2);

            var sut = new PersistentValue<string>(
                "value1",
                null,
                null,
                serializer);

            var invalidatedCount = 0;
            sut.OnInvalidated += (_, _) => invalidatedCount++;

            sut.Value = "value2";

            invalidatedCount.Should().Be(1);
        }
    }

    public class Reset
    {
        [Fact]
        public void ShouldRestoreDefaultValue()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier1 = Substitute.For<INotifySerializedValueChanged>();
            var notifier2 = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("default", typeof(string)).Returns(notifier1);
            serializer.GetSerializedValueChangedNotifier("changed", typeof(string)).Returns(notifier2);

            var sut = new PersistentValue<string>(
                "default",
                null,
                null,
                serializer)
            {
                Value = "changed"
            };

            sut.Reset();

            sut.Value.Should().Be("default");
        }

        [Fact]
        public void ShouldDoNothingWhenNoDefaultValueExists()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();

            transcoder.DecodeElement(Arg.Any<object>(), typeof(string)).Returns("decoded");

            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));

            serializer.DeserializeObject(Arg.Any<DeserializationRequest>()).Returns("value");

            serializer.GetSerializedValueChangedNotifier("value", typeof(string)).Returns(notifier);

            var sut = PersistentValue<string>.FromEncodedValue(
                "encoded",
                null,
                null,
                serializer,
                transcoder);

            var action = sut.Reset;

            action.Should().NotThrow();
        }
    }

    public class Encode
    {
        [Fact]
        public void ShouldSerializeAndEncodeValue()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("value", typeof(string)).Returns(notifier);
            serializer.SerializeObject("value", typeof(string)).Returns("serialized");
            transcoder.EncodeElement("serialized").Returns("encoded");

            var sut = new PersistentValue<string>(
                "value",
                null,
                null,
                serializer);

            var result = sut.Encode(transcoder);

            result.Should().Be("encoded");
        }

        [Fact]
        public void ShouldThrowWhenTranscoderReturnsNull()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("value", typeof(string)).Returns(notifier);
            serializer.SerializeObject("value", typeof(string)).Returns("serialized");
            transcoder.EncodeElement("serialized").Returns((object?)null);

            var sut = new PersistentValue<string>(
                "value",
                null,
                null,
                serializer);

            var action = () => sut.Encode(transcoder);

            action.Should().Throw<Exception>();
        }
    }

    public class Decode
    {
        [Fact]
        public void ShouldUpdateToDecodedValue()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var notifier1 = Substitute.For<INotifySerializedValueChanged>();
            var notifier2 = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("existing", typeof(string)).Returns(notifier1);
            serializer.GetSerializedValueChangedNotifier("new", typeof(string)).Returns(notifier2);
            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));
            transcoder.DecodeElement("encoded", typeof(string)).Returns("decoded");
            serializer.DeserializeObject(
                    Arg.Is<DeserializationRequest>(r =>
                        Equals(r.ExistingInstance, "existing")))
                .Returns("new");

            var sut = new PersistentValue<string>(
                "existing",
                null,
                null,
                serializer);

            sut.Decode(transcoder, "encoded");

            sut.Value.Should().Be("new");
        }

        [Fact]
        public void ShouldThrowDeserializationErrorWhenDecodedValueIsNull()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));
            transcoder.DecodeElement(Arg.Any<object>(), typeof(string)).Returns((object?)null);
            var notifier = Substitute.For<INotifySerializedValueChanged>();
            serializer.GetSerializedValueChangedNotifier("value", typeof(string))
                .Returns(notifier);

            var sut = new PersistentValue<string>(
                "value",
                null,
                null,
                serializer);

            var action = () => sut.Decode(transcoder, "encoded");
            action.Should().Throw<DeserializationError>();
        }

        [Fact]
        public void ShouldThrowDeserializationErrorWhenWrongTypeReturned()
        {
            var serializer = Substitute.For<ISerializer>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            serializer.GetSerializedType(typeof(string)).Returns(typeof(string));
            transcoder.DecodeElement(Arg.Any<object>(), typeof(string)).Returns("decoded");
            serializer.DeserializeObject(Arg.Any<DeserializationRequest>()).Returns(123);
            var notifier = Substitute.For<INotifySerializedValueChanged>();
            serializer.GetSerializedValueChangedNotifier("value", typeof(string)).Returns(notifier);

            var sut = new PersistentValue<string>(
                "value",
                null,
                null,
                serializer);

            var action = () => sut.Decode(transcoder, "encoded");
            action.Should().Throw<DeserializationError>();
        }
    }

    public class OnInvalidated
    {
        [Fact]
        public void ShouldRaiseWhenSerializedValueChanges()
        {
            var serializer = Substitute.For<ISerializer>();
            var notifier = Substitute.For<INotifySerializedValueChanged>();

            serializer.GetSerializedValueChangedNotifier("value", typeof(string)).Returns(notifier);

            var sut = new PersistentValue<string>(
                "value",
                null,
                null,
                serializer);

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            notifier.SerializedValueChanged +=
                Raise.Event<EventHandler>(notifier, EventArgs.Empty);

            raised.Should().BeTrue();
        }
    }
}