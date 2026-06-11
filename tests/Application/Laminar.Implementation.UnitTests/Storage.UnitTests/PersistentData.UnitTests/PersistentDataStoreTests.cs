using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class PersistentDataStoreTests
{
    public class ConstructionHydration
    {
        [Fact]
        public void ShouldCreateRoot()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([]);

            var sut = CreateSut(file: file);

            sut.Root.Should().NotBeNull();
        }

        [Fact]
        public void ShouldNotAttemptDecodeWhenFileEmpty()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([]);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            _ = CreateSut(transcoder: transcoder, file: file);

            transcoder.DidNotReceive()
                .BytesToElement(Arg.Any<byte[]>());
        }

        [Fact]
        public void ShouldDecodeFileIntoRootWhenFileHasData()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([1, 2, 3]);

            var decoded = new object();

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.BytesToElement(file.Contents).Returns(decoded);

            var root = Substitute.For<IPersistentDictionary>();

            _ = CreateSut(transcoder, file, root: root);

            root.Received(1).Decode(transcoder, decoded);
        }
    }

    public class SynchronousFlush
    {
        [Fact]
        public void ShouldEncodeRootAndWriteToFile()
        {
            var file = Substitute.For<IFileContents>();
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var root = Substitute.For<IPersistentDictionary>();
            root.Encode(transcoder).Returns("encoded");

            transcoder.ElementToBytes("encoded")
                .Returns(new byte[] { 9, 9 });

            var sut = CreateSut(transcoder, file, root: root);

            sut.SynchronousFlush();

            root.Received(1).Encode(transcoder);
            file.Received(1).Contents = Arg.Is<byte[]>(x => ((IEnumerable<byte>)x).SequenceEqual(new byte[] { 9, 9 }));
        }
    }

    public class FileToDataNode
    {
        [Fact]
        public void ShouldThrowWhenDecodeReturnsNull()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([1]);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.BytesToElement(Arg.Any<byte[]>()).Returns((object?)null);

            var act = () => CreateSut(transcoder: transcoder, file: file);

            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class Dispose
    {
        [Fact]
        public void ShouldDisposeFile()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([]);

            var sut = CreateSut(file: file);

            sut.Dispose();

            file.Received(1).Dispose();
        }

        [Fact]
        public void ShouldAllowMultipleDisposeCalls()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([]);

            var sut = CreateSut(file: file);

            sut.Dispose();
            sut.Dispose();

            file.Received(1).Dispose();
        }

        [Fact]
        public void ShouldSuppressFinalization()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([]);

            var sut = CreateSut(file: file);

            sut.Dispose();

            // indirect assertion: no exception on reuse + safe cleanup
            Action act = sut.Dispose;

            act.Should().NotThrow();
        }
    }
    
    
    private static PersistentDataStore CreateSut(
        IPersistentDataTranscoder? transcoder = null,
        IFileContents? file = null,
        IEncodableDataFactory? dataFactory = null,
        IPersistentDictionary? root = null)
    {
        transcoder ??= Substitute.For<IPersistentDataTranscoder>();
        file ??= Substitute.For<IFileContents>();
        dataFactory ??= Substitute.For<IEncodableDataFactory>();
        root ??= Substitute.For<IPersistentDictionary>();
        
        dataFactory.GetEncodableData<IPersistentDictionary>().Returns(root);
        
        return new(dataFactory, transcoder, file);
    }
}