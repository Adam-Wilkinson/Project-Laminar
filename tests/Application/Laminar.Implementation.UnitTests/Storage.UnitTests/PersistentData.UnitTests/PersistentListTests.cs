using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentListTests
{
    public class AddNext
    {
        [Fact]
        public void ShouldCreateNewDataPointUsingFactory()
        {
            var dataPoint = Substitute.For<IPersistentDataPoint>();
            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(dataPoint);

            var sut = new PersistentList(factory);

            var result = sut.AddNext();

            result.Should().BeSameAs(dataPoint);
        }

        [Fact]
        public void ShouldAddValueToList()
        {
            var dataPoint = Substitute.For<IPersistentDataPoint>();
            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(dataPoint);

            var sut = new PersistentList(factory);

            sut.AddNext();

            sut.Count.Should().Be(1);
            sut[0].Should().BeSameAs(dataPoint);
        }

        [Fact]
        public void ShouldSubscribeToInvalidatedEvent()
        {
            var dataPoint = Substitute.For<IPersistentDataPoint>();
            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(dataPoint);

            var sut = new PersistentList(factory);

            sut.AddNext();

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            dataPoint.OnInvalidated += Raise.Event<EventHandler>(dataPoint, EventArgs.Empty);

            raised.Should().BeTrue();
        }
    }
    
    public class Remove
    {
        [Fact]
        public void ShouldReturnFalseWhenItemDoesNotExist()
        {
            var sut = new PersistentList(Substitute.For<IEncodableDataFactory>());

            var result = sut.Remove(Substitute.For<IPersistentDataPoint>());

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnTrueWhenItemExists()
        {
            var item = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item);

            var sut = new PersistentList(factory);
            sut.AddNext();

            var result = sut.Remove(item);

            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldRemoveItem()
        {
            var item = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item);

            var sut = new PersistentList(factory);
            sut.AddNext();

            sut.Remove(item);

            sut.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldRaiseInvalidatedWhenItemRemoved()
        {
            var item = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item);

            var sut = new PersistentList(factory);
            sut.AddNext();

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            sut.Remove(item);

            raised.Should().BeTrue();
        }

        [Fact]
        public void ShouldStopListeningToRemovedItem()
        {
            var item = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item);

            var sut = new PersistentList(factory);
            sut.AddNext();

            sut.Remove(item);

            var count = 0;
            sut.OnInvalidated += (_, _) => count++;

            item.OnInvalidated += Raise.Event<EventHandler>(item, EventArgs.Empty);

            count.Should().Be(0);
        }
    }

    public class Clear
    {
        [Fact]
        public void ShouldRemoveAllItems()
        {
            var item1 = Substitute.For<IPersistentDataPoint>();
            var item2 = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item1, item2);

            var sut = new PersistentList(factory);

            sut.AddNext();
            sut.AddNext();

            sut.Clear();

            sut.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldRaiseInvalidated()
        {
            var sut = new PersistentList(Substitute.For<IEncodableDataFactory>());

            var raised = false;
            sut.OnInvalidated += (_, _) => raised = true;

            sut.Clear();

            raised.Should().BeTrue();
        }

        [Fact]
        public void ShouldStopListeningToChildren()
        {
            var item = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item);

            var sut = new PersistentList(factory);

            sut.AddNext();
            sut.Clear();

            var count = 0;
            sut.OnInvalidated += (_, _) => count++;

            item.OnInvalidated += Raise.Event<EventHandler>(item, EventArgs.Empty);

            count.Should().Be(0);
        }
    }
    
    public class Encode
    {
        [Fact]
        public void ShouldEncodeAllItems()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var item1 = Substitute.For<IPersistentDataPoint>();
            var item2 = Substitute.For<IPersistentDataPoint>();

            item1.Encode(transcoder).Returns("one");
            item2.Encode(transcoder).Returns("two");

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item1, item2);

            var sut = new PersistentList(factory);

            sut.AddNext();
            sut.AddNext();

            var result = (object[])sut.Encode(transcoder);

            result.Should().Equal("one", "two");
        }

        [Fact]
        public void ShouldReturnEmptyArrayWhenListIsEmpty()
        {
            var sut = new PersistentList(Substitute.For<IEncodableDataFactory>());

            var result = (object[])sut.Encode(Substitute.For<IPersistentDataTranscoder>());

            result.Should().BeEmpty();
        }
    }

    public class Decode
    {
        [Fact]
        public void ShouldDecodeExistingItemsWhenCountsMatch()
        {
            var item1 = Substitute.For<IPersistentDataPoint>();
            var item2 = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item1, item2);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var sut = new PersistentList(factory);

            sut.AddNext();
            sut.AddNext();

            transcoder.DecodeElement("encoded", typeof(List<object>))
                .Returns(new List<object> { "one", "two" });

            sut.Decode(transcoder, "encoded");

            item1.Received(1).Decode(transcoder, "one");
            item2.Received(1).Decode(transcoder, "two");
        }
        
        [Fact]
        public void ShouldCreateAdditionalItemsWhenDecodedListIsLarger()
        {
            var item1 = Substitute.For<IPersistentDataPoint>();
            var item2 = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item1, item2);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var sut = new PersistentList(factory);

            sut.AddNext();

            transcoder.DecodeElement("encoded", typeof(List<object>))
                .Returns(new List<object>
                {
                    "one",
                    "two"
                });

            sut.Decode(transcoder, "encoded");

            sut.Count.Should().Be(2);

            item1.Received(1).Decode(transcoder, "one");
            item2.Received(1).Decode(transcoder, "two");
        }
        
        [Fact]
        public void ShouldRemoveExcessItemsWhenDecodedListIsSmaller()
        {
            var item1 = Substitute.For<IPersistentDataPoint>();
            var item2 = Substitute.For<IPersistentDataPoint>();
            var item3 = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item1, item2, item3);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var sut = new PersistentList(factory);

            sut.AddNext();
            sut.AddNext();
            sut.AddNext();

            transcoder.DecodeElement("encoded", typeof(List<object>))
                .Returns(new List<object>
                {
                    "one"
                });

            sut.Decode(transcoder, "encoded");

            sut.Count.Should().Be(1);

            item1.Received(1).Decode(transcoder, "one");
            item2.DidNotReceive().Decode(Arg.Any<IPersistentDataTranscoder>(), Arg.Any<object>());
            item3.DidNotReceive().Decode(Arg.Any<IPersistentDataTranscoder>(), Arg.Any<object>());
        }
        
        [Fact]
        public void ShouldCreateAllItemsWhenInitiallyEmpty()
        {
            var item1 = Substitute.For<IPersistentDataPoint>();
            var item2 = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(item1, item2);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            transcoder.DecodeElement("encoded", typeof(List<object>))
                .Returns(new List<object>
                {
                    "one",
                    "two"
                });

            var sut = new PersistentList(factory);

            sut.Decode(transcoder, "encoded");

            sut.Count.Should().Be(2);

            item1.Received(1).Decode(transcoder, "one");
            item2.Received(1).Decode(transcoder, "two");
        }
    }
    
    public class OnInvalidated
    {
        [Fact]
        public void ShouldPropagateChildInvalidation()
        {
            var child = Substitute.For<IPersistentDataPoint>();

            var factory = Substitute.For<IEncodableDataFactory>();
            factory.GetDataPoint().Returns(child);

            var sut = new PersistentList(factory);

            sut.AddNext();

            object? sender = null;
            EventArgs? args = null;

            sut.OnInvalidated += (s, e) =>
            {
                sender = s;
                args = e;
            };

            child.OnInvalidated += Raise.Event<EventHandler>(child, EventArgs.Empty);

            sender.Should().Be(child);
            args.Should().BeSameAs(EventArgs.Empty);
        }
    }
}