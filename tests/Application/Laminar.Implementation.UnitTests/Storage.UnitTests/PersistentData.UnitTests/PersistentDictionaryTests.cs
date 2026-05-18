using FluentAssertions;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDictionaryTests
{
    [Fact]
    public void ChildNodeShouldHaveCorrectOwner()
    {
        var rootOwner = Substitute.For<IPersistentDataValueOwner>();

        var node = CreateNode();
        node.Owner = rootOwner;

        var child = node["child"].SetDefaultAndGet(CreateNode());

        child.Value.Owner.Should().Be(node);
    }
    
    public class GetPersistentData
    {
        [Fact]
        public void ShouldCreateValueIfMissing()
        {
            var sut = CreateNode();
            var value = sut.GetPersistentData("key");
            value.Should().NotBeNull();
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

            owner.Received(1).OnChildValueInvalidated();
            
            sut.OnChildValueInvalidated();

            owner.Received(2).OnChildValueInvalidated();
        }

        [Fact]
        public void ShouldDoNothingIfNoOwner()
        {
            var sut = CreateNode();

            sut.Invoking(x => x.OnChildValueInvalidated())
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
            var owner = CreateNode();

            sut.Owner = owner;
            owner.RegisterChildNode(sut);
            
            using var mon = sut.Monitor();

            owner.OnTranscoderChanged();

            mon.Should().Raise(nameof(sut.TranscoderChanged));
        }

        [Fact]
        public void ShouldUnsubscribeFromPreviousOwner()
        {
            var sut = CreateNode();
            var owner1 = CreateNode();
            var owner2 = CreateNode();

            sut.Owner = owner1;
            sut.Owner = owner2;

            using var mon = sut.Monitor();

            owner1.OnTranscoderChanged();

            mon.Should().NotRaise(nameof(sut.TranscoderChanged));
        }
    }
    
    public class RemoveValue
    {
        [Fact]
        public void ShouldRemoveValue()
        {
            var sut = CreateNode();

            sut["key"].SetDefaultAndGet(10);

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

            sut["key"].SetDefaultAndGet(10);

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

            sut["key"].SetDefaultAndGet(42);

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

            var result = sut["key"].SetDefaultAndGet(10);

            result.Value.Should().Be(10);
            sut.InternalValues.Should().ContainKey("key");
        }

        [Fact]
        public void ShouldRaiseErrorIfValueExists()
        {
            var sut = CreateNode();

            sut["key"].SetDefaultAndGet(10);

            sut.Invoking(x => x["key"].SetDefaultAndGet(20))
                .Should().Throw<InvalidOperationException>();
        }
    }
    
    public class GetOrCreateChild
    {
        [Fact]
        public void ShouldCreateNewChildIfNotInitialized()
        {
            var sut = CreateNode();

            var child = sut["child"];

            child.Should().NotBeNull();
            sut.InternalValues.Should().ContainKey("child");
        }

        [Fact]
        public void ShouldReturnSameChildIfAlreadyInitialized()
        {
            var sut = CreateNode();

            var first = sut["child"];
            var second = sut["child"];

            second.Should().BeSameAs(first);
        }

        [Fact]
        public void ShouldThrowIfValueExistsButIsWrongType()
        {
            var sut = CreateNode();

            var value = sut.GetPersistentData("child");
            value.SetDefaultAndGet(123, typeof(int));

            Assert.Throws<InvalidCastException>(() => sut["child"].GetValue<string>());
        }
    }
    
    private static PersistentDictionary CreateNode(
        ISerializer? serializer = null,
        ILogger<PersistentDataPoint>? logger = null,
        IExceptionHandler? exceptionHandler = null)
    {
        serializer ??= Substitute.For<ISerializer>();
        logger ??= Substitute.For<ILogger<PersistentDataPoint>>();
        exceptionHandler ??= Substitute.For<IExceptionHandler>();
        
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ISerializer)).Returns(serializer);
        serviceProvider.GetService(typeof(ILogger<PersistentDataPoint>)).Returns(logger);
        serviceProvider.GetService(typeof(IExceptionHandler)).Returns(exceptionHandler);
        serviceProvider.GetService(typeof(IServiceProvider)).Returns(serviceProvider);
        
        return new PersistentDictionary(serviceProvider);
    }
}