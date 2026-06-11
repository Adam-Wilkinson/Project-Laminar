using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDictionaryTests
{
    public class Indexer
    {
        [Fact]
        public void ShouldCreateValueWhenKeyDoesNotExist()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            var value = sut["key"];

            value.Should().NotBeNull();
            sut.Count.Should().Be(1);
            sut.ContainsKey("key").Should().BeTrue();
        }

        [Fact]
        public void ShouldReturnExistingValueWhenKeyAlreadyExists()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            var value1 = sut["key"];
            var value2 = sut["key"];

            value2.Should().BeSameAs(value1);
            sut.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldRaiseInvalidatedWhenCreatingNewValue()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            _ = sut["key"];

            raised.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotRaiseInvalidatedWhenReturningExistingValue()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());
            _ = sut["key"];

            var count = 0;
            sut.OnInvalidated += (_, _) => count++;

            _ = sut["key"];

            count.Should().Be(0);
        }
    }
    
    public class Remove
    {
        [Fact]
        public void ShouldReturnFalseWhenKeyDoesNotExist()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            var result = sut.Remove("missing");

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrueWhenKeyExists()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());
            _ = sut["key"];

            var result = sut.Remove("key");

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldRemoveValue()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());
            _ = sut["key"];

            sut.Remove("key");

            sut.ContainsKey("key").Should().BeFalse();
        }

        [Fact]
        public void ShouldRaiseInvalidatedWhenValueRemoved()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());
            _ = sut["key"];

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            sut.Remove("key");

            raised.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotRaiseInvalidatedWhenKeyDoesNotExist()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            var count = 0;
            sut.OnInvalidated += (_, _) => count++;

            sut.Remove("missing");

            count.Should().Be(0);
        }
    }
    
    public class Clear
    {
        [Fact]
        public void ShouldRemoveAllValues()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            _ = sut["a"];
            _ = sut["b"];

            sut.Clear();

            sut.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldRaiseInvalidated()
        {
            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            var raisedCount = 0;
            sut.OnInvalidated += (_, _) => raisedCount++;

            sut.Clear();

            raisedCount.Should().Be(1);
        }
    }
    
    public class Encode
    {
        [Fact]
        public void ShouldEncodeAllValues()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
                
            var value1 = Substitute.For<IPersistentDataPoint>();
            var value2 = Substitute.For<IPersistentDataPoint>();

            value1.Encode(transcoder).Returns("encoded1");
            value2.Encode(transcoder).Returns("encoded2");

            var sut = new PersistentDictionary(dataFactory);
            dataFactory.GetDataPoint().Returns(value1);
            _ = sut["one"];
                
            dataFactory.GetDataPoint().Returns(value2);
            _ = sut["two"];

            transcoder.EncodeElement(
                    Arg.Is<Dictionary<string, object>>(x =>
                        (string)x["one"] == "encoded1" &&
                        (string)x["two"] == "encoded2"))
                .Returns("final");

            var result = sut.Encode(transcoder);

            result.Should().Be("final");
        }

        [Fact]
        public void ShouldThrowWhenTranscoderReturnsNull()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var sut = new PersistentDictionary(Substitute.For<IEncodableDataFactory>());

            transcoder.EncodeElement(Arg.Any<object>()).Returns((object?)null);

            var action = () => sut.Encode(transcoder);

            action.Should().Throw<InvalidOperationException>();
        }
    }
    
    public class Decode
    {
        [Fact]
        public void ShouldDecodeAllEntriesIntoPersistentData()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            
            var value1 = Substitute.For<IPersistentDataPoint>();
            var value2 = Substitute.For<IPersistentDataPoint>();

            var sut = new PersistentDictionary(dataFactory);
            dataFactory.GetDataPoint().Returns(value1);
            _ = sut["one"];
            dataFactory.GetDataPoint().Returns(value2);
            _ = sut["two"];

            var decoded = new Dictionary<string, object>
            {
                ["one"] = "value1",
                ["two"] = "value2"
            };

            transcoder.DecodeElement("encoded", typeof(Dictionary<string, object>)).Returns(decoded);

            sut.Decode(transcoder, "encoded");

            value1.Received(1).Decode(transcoder, "value1");
            value2.Received(1).Decode(transcoder, "value2");
        }

        [Fact]
        public void ShouldCreateMissingPersistentDataPoints()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetDataPoint().Returns(Substitute.For<IPersistentDataPoint>());
            
            var decoded = new Dictionary<string, object>
            {
                ["one"] = "value1"
            };

            transcoder.DecodeElement("encoded", typeof(Dictionary<string, object>)).Returns(decoded);

            var sut = new PersistentDictionary(dataFactory);

            sut.Decode(transcoder, "encoded");

            sut.ContainsKey("one").Should().BeTrue();
        }
    }
    
    public class OnInvalidated
    {
        [Fact]
        public void ShouldRaiseWhenChildRaisesInvalidated()
        {
            var child = Substitute.For<IPersistentDataPoint>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetDataPoint().Returns(child);
            var sut = new PersistentDictionary(dataFactory);
            _ = sut["key"];
            
            object? sender = null;
            EventArgs? args = null;

            sut.OnInvalidated += (s, e) =>
            {
                sender = s;
                args = e;
            };

            child.OnInvalidated +=
                Raise.Event<EventHandler>(child, EventArgs.Empty);

            sender.Should().Be(child);
            args.Should().BeSameAs(EventArgs.Empty);
        }

        [Fact]
        public void ShouldStopListeningAfterRemove()
        {
            var child = Substitute.For<IPersistentDataPoint>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetDataPoint().Returns(child);
            var sut = new PersistentDictionary(dataFactory);
            _ = sut["key"];
            
            sut.Remove("key");

            var count = 0;
            sut.OnInvalidated += (_, _) => count++;

            child.OnInvalidated +=
                Raise.Event<EventHandler>(child, EventArgs.Empty);

            count.Should().Be(0);
        }

        [Fact]
        public void ShouldStopListeningAfterClear()
        {
            var child = Substitute.For<IPersistentDataPoint>();
            var dataFactory = Substitute.For<IEncodableDataFactory>();
            dataFactory.GetDataPoint().Returns(child);
            var sut = new PersistentDictionary(dataFactory);
            _ = sut["key"];

            sut.Clear();

            var count = 0;
            sut.OnInvalidated += (_, _) => count++;

            child.OnInvalidated +=
                Raise.Event<EventHandler>(child, EventArgs.Empty);

            count.Should().Be(0);
        }
    }
}
