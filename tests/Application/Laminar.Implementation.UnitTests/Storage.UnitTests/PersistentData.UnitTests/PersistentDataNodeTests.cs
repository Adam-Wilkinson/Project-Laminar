using FluentAssertions;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDataNodeTests
{
    [Fact]
    public void ChildNodeShouldHaveCorrectOwner()
    {
        var rootOwner = Substitute.For<IPersistentDataValueOwner>();

        var node = CreateNode();
        node.Owner = rootOwner;

        var child = node.GetOrCreateChild("child");

        ((PersistentDataNode)child).Owner.Should().Be(node);
    }
    
    public class GetPersistentData
    {
        [Fact]
        public void ShouldCreateValueIfMissing()
        {
            var sut = CreateNode();

            var value = sut.GetPersistentData("key");

            value.Should().NotBeNull();
            value.Name.Should().Be("key");
        }

        [Fact]
        public void ShouldReturnSameInstanceIfExists()
        {
            var sut = CreateNode();

            var first = sut.GetPersistentData("key");
            var second = sut.GetPersistentData("key");

            second.Should().BeSameAs(first);
        }
    }
    
    public class OnChildValueChanged
    {
        [Fact]
        public void ShouldForwardToOwner()
        {
            var sut = CreateNode();
            var owner = Substitute.For<IPersistentDataValueOwner>();

            sut.Owner = owner;

            sut.OnChildValueChanged();

            owner.Received(1).OnChildValueChanged();
        }

        [Fact]
        public void ShouldDoNothingIfNoOwner()
        {
            var sut = CreateNode();

            sut.Invoking(x => x.OnChildValueChanged())
                .Should().NotThrow();
        }
    }
    
    public class Owner
    {
        [Fact]
        public void ShouldExposeTranscoderFromOwner()
        {
            var owner = Substitute.For<IPersistentDataValueOwner>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            owner.Transcoder.Returns(transcoder);

            var sut = CreateNode();

            sut.Owner = owner;

            sut.Transcoder.Should().Be(transcoder);
        }

        [Fact]
        public void ShouldRaiseTranscoderChangedWhenOwnerSet()
        {
            var sut = CreateNode();
            var owner = Substitute.For<IPersistentDataValueOwner>();

            using var mon = sut.Monitor();

            sut.Owner = owner;

            mon.Should().Raise(nameof(sut.TranscoderChanged));
        }

        [Fact]
        public void ShouldPropagateOwnerTranscoderChanged()
        {
            var sut = CreateNode();
            var owner = Substitute.For<IPersistentDataValueOwner>();

            sut.Owner = owner;

            using var mon = sut.Monitor();

            owner.TranscoderChanged += Raise.Event<EventHandler>(owner, EventArgs.Empty);

            mon.Should().Raise(nameof(sut.TranscoderChanged));
        }

        [Fact]
        public void ShouldUnsubscribeFromPreviousOwner()
        {
            var sut = CreateNode();
            var owner1 = Substitute.For<IPersistentDataValueOwner>();
            var owner2 = Substitute.For<IPersistentDataValueOwner>();

            sut.Owner = owner1;
            sut.Owner = owner2;

            using var mon = sut.Monitor();

            owner1.TranscoderChanged += Raise.Event<EventHandler>(owner1, EventArgs.Empty);

            mon.Should().NotRaise(nameof(sut.TranscoderChanged));
        }
    }
    
    public class RemoveValue
    {
        [Fact]
        public void ShouldRemoveValue()
        {
            var sut = CreateNode();

            sut.InitializeDefaultValue("key", 10);

            var result = sut.RemoveValue("key");

            result.Should().BeTrue();
            sut.InternalValues.Should().NotContainKey("key");
        }

        [Fact]
        public void ShouldReturnFalseIfKeyMissing()
        {
            var sut = CreateNode();

            var result = sut.RemoveValue("missing");

            result.Should().BeFalse();
        }
    }
    
    public class SetValue
    {
        [Fact]
        public void ShouldReturnFalseIfKeyMissing()
        {
            var sut = CreateNode();

            var result = sut.SetValue("missing", 10);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldSetValueIfExists()
        {
            var sut = CreateNode();

            sut.InitializeDefaultValue("key", 10);

            var result = sut.SetValue("key", 20);

            result.Should().BeTrue();
            sut.TryGetValue<int>("key")!.Value.Should().Be(20);
        }
    }
    
    public class TryGetValue
    {
        [Fact]
        public void ShouldReturnNullIfMissing()
        {
            var sut = CreateNode();

            var result = sut.TryGetValue<int>("missing");

            result.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnValueIfPresent()
        {
            var sut = CreateNode();

            sut.InitializeDefaultValue("key", 42);

            var result = sut.TryGetValue<int>("key");

            result.Should().NotBeNull();
            result!.Value.Should().Be(42);
        }
    }
    
    public class InitializeDefaultValue
    {
        [Fact]
        public void ShouldInitializeAndReturnObservable()
        {
            var sut = CreateNode();

            var result = sut.InitializeDefaultValue("key", 10);

            result.Value.Should().Be(10);
            sut.InternalValues.Should().ContainKey("key");
        }

        [Fact]
        public void ShouldRaiseErrorIfValueExists()
        {
            var sut = CreateNode();

            sut.InitializeDefaultValue("key", 10);

            sut.Invoking(x => x.InitializeDefaultValue("key", 20))
                .Should().Throw<InvalidOperationException>();
        }
    }
    
    public class GetOrCreateChild
    {
        [Fact]
        public void ShouldCreateNewChildIfNotInitialized()
        {
            var sut = CreateNode();

            var child = sut.GetOrCreateChild("child");

            child.Should().NotBeNull();
            sut.InternalValues.Should().ContainKey("child");
        }

        [Fact]
        public void ShouldReturnSameChildIfAlreadyInitialized()
        {
            var sut = CreateNode();

            var first = sut.GetOrCreateChild("child");
            var second = sut.GetOrCreateChild("child");

            second.Should().BeSameAs(first);
        }

        [Fact]
        public void ShouldThrowIfValueExistsButIsWrongType()
        {
            var sut = CreateNode();

            var value = sut.GetPersistentData("child");
            value.Initialize(123, typeof(int));

            Assert.Throws<ArgumentException>(() => sut.GetOrCreateChild("child"));
        }
    }
    
    private static PersistentDataNode CreateNode(
        ISerializer? serializer = null,
        ILogger<PersistentDataValue>? logger = null)
    {
        serializer ??= Substitute.For<ISerializer>();
        logger ??= Substitute.For<ILogger<PersistentDataValue>>();

        return new PersistentDataNode(serializer, logger);
    }
}