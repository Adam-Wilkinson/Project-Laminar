using FluentAssertions;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDataStoreTests
{
    public class FeedbackLoop
    {
        [Fact]
        public void FileUpdateShouldNotImmediatelyRewriteFile()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var file = Substitute.For<IFileContents>();
            var serializer = Substitute.For<ISerializer>();

            var store = CreateStore(transcoder, file, serializer);

            var bytes = new byte[] { 1, 2 };
            var decoded = new Dictionary<string, object>();

            file.Contents.Returns(bytes);
            transcoder.FromBytes<Dictionary<string, object>>(bytes).Returns(decoded);

            file.ContentsChanged += Raise.Event<EventHandler>(file, EventArgs.Empty);

            // If this fails, you have a loop
            file.DidNotReceive().Contents = Arg.Any<byte[]>();
        }
    }
    
    public class Integration
    {
        [Fact]
        public void ChangingValueShouldWriteToFile()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var file = Substitute.For<IFileContents>();
            var serializer = Substitute.For<ISerializer>();

            var store = CreateStore(transcoder, file, serializer);

            var root = store.Root;

            var serialized = new object();
            var bytes = new byte[] { 9, 9, 9 };

            serializer.SerializeObject(root, typeof(PersistentDataNode))
                .Returns(serialized);

            transcoder.ToBytes(serialized).Returns(bytes);

            root.InitializeDefaultValue("key", 10);
            root.SetValue("key", 20);

            file.Received().Contents = bytes;
        }
    }
    
    public class FileToDataNode
    {
        [Fact]
        public void ShouldDeserializeFileIntoRoot()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var decoded = new Dictionary<string, object>();
            
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var file = Substitute.For<IFileContents>();
            var serializer = Substitute.For<ISerializer>();
            file.Contents.Returns(bytes);
            transcoder.FromBytes<Dictionary<string, object>>(bytes).Returns(decoded);

            var store = CreateStore(transcoder, file, serializer);

            serializer.Received(1).DeserializeObject(decoded, typeof(PersistentDataNode), store.Root);
        }
    }
    
    public class OnChildValueChanged
    {
        [Fact]
        public void ShouldSerializeRootAndWriteToFile()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            var file = Substitute.For<IFileContents>();
            var serializer = Substitute.For<ISerializer>();

            var store = CreateStore(transcoder, file, serializer);

            var serialized = new object();
            var bytes = new byte[] { 1, 2, 3 };

            serializer.SerializeObject(store.Root, typeof(PersistentDataNode))
                .Returns(serialized);

            transcoder.ToBytes(serialized).Returns(bytes);

            store.OnChildValueChanged();

            serializer.Received(1).SerializeObject(store.Root, typeof(PersistentDataNode));
            transcoder.Received(1).ToBytes(serialized);
            file.Contents = bytes;
        }
    }
    
    public class Construction
    {
        [Fact]
        public void ShouldSetRootOwner()
        {
            var store = CreateStore();

            store.Root.Should().NotBeNull();
            store.Root.Should().BeOfType<PersistentDataNode>();

            var root = (PersistentDataNode)store.Root;
            root.Owner.Should().Be(store);
        }

        // [Fact]
        // public void ShouldSubscribeToFileChanges()
        // {
        //     var file = Substitute.For<IFileContents>();
        //
        //     CreateStore(file: file);
        //
        //     file.Received().ContentsChanged += Arg.Any<EventHandler>();
        // }
    }
    
    private static PersistentDataStore CreateStore(
        IPersistentDataTranscoder? transcoder = null,
        IFileContents? file = null,
        ISerializer? serializer = null,
        ILogger<PersistentDataValue>? logger = null)
    {
        transcoder ??= Substitute.For<IPersistentDataTranscoder>();
        file ??= Substitute.For<IFileContents>();
        serializer ??= Substitute.For<ISerializer>();
        logger ??= Substitute.For<ILogger<PersistentDataValue>>();

        return new PersistentDataStore(transcoder, file, Substitute.For<IExceptionHandler>(), serializer, logger);
    }
}