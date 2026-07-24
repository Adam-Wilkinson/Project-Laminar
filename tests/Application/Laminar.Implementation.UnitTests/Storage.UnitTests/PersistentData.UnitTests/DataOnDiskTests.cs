using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.PersistentData;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.PersistentData.UnitTests;

public class DataOnDiskTests
{
    public class ConstructionHydration
    {
        [Fact]
        public void ShouldExposeProvidedData()
        {
            var data = Substitute.For<IEncodableData>();

            var sut = CreateSut(data: data);

            sut.Data.Should().BeSameAs(data);
        }

        [Fact]
        public void ShouldNotAttemptDecodeWhenFileEmpty()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([]);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            _ = CreateSut(transcoder: transcoder, file: file);

            transcoder.DidNotReceive().BytesToElement(Arg.Any<byte[]>());
        }

        [Fact]
        public void ShouldDecodeFileWhenFileHasContents()
        {
            byte[] fileContents = [1, 2, 3];
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns(fileContents);

            var decoded = new object();

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.BytesToElement(fileContents).Returns(decoded);

            var data = Substitute.For<IEncodableData>();

            _ = CreateSut(transcoder: transcoder, file: file, data: data);

            data.Received(1).Decode(transcoder, decoded);
        }

        [Fact]
        public void ShouldThrowWhenDecodeReturnsNull()
        {
            var file = Substitute.For<IFileContents>();
            file.Contents.Returns([1]);

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.BytesToElement(file.Contents).Returns((object?)null);

            Action act = () => CreateSut(transcoder: transcoder, file: file);

            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class SynchronousFlush
    {
        [Fact]
        public void ShouldEncodeData()
        {
            var data = Substitute.For<IEncodableData>();
            data.Encode(Arg.Any<IPersistentDataTranscoder>()).Returns("encoded");

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.ElementToBytes("encoded").Returns([1, 2]);

            var sut = CreateSut(data: data, transcoder: transcoder);

            sut.SynchronousFlush();

            data.Received(1).Encode(transcoder);
        }

        [Fact]
        public void ShouldWriteEncodedBytesToFile()
        {
            var file = Substitute.For<IFileContents>();

            var data = Substitute.For<IEncodableData>();
            data.Encode(Arg.Any<IPersistentDataTranscoder>()).Returns("encoded");

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.ElementToBytes("encoded").Returns([9, 8, 7]);

            var sut = CreateSut(data: data, transcoder: transcoder, file: file);

            sut.SynchronousFlush();

            file.Received(1).Contents =
                Arg.Is<byte[]>(x => x != null && ((IEnumerable<byte>)x).SequenceEqual(new byte[] { 9, 8, 7 }));
        }
    }

    public class Transcoder
    {
        [Fact]
        public void ShouldReturnProvidedTranscoder()
        {
            var transcoder = Substitute.For<IPersistentDataTranscoder>();

            var sut = CreateSut(transcoder: transcoder);

            sut.Transcoder.Should().BeSameAs(transcoder);
        }

        [Fact]
        public void ShouldUpdateTranscoder()
        {
            var sut = CreateSut();

            var replacement = Substitute.For<IPersistentDataTranscoder>();

            sut.Transcoder = replacement;

            sut.Transcoder.Should().BeSameAs(replacement);
        }
    }

    public class Location
    {
        [Fact]
        public void ShouldReturnCurrentFilePath()
        {
            var path = new FileSystemPath("file1");

            var file = Substitute.For<IFileContents>();
            file.Path.Returns(path);

            var sut = CreateSut(file: file);

            sut.Location.Should().Be(path);
        }

        [Fact]
        public void ShouldDisposePreviousFileWhenLocationChanges()
        {
            var oldFile = Substitute.For<IFileContents>();

            var newFile = Substitute.For<IFileContents>();

            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.GetFileContents(Arg.Any<FileSystemPath>())
                .Returns(oldFile, newFile);

            var sut = CreateSut(fileSystem: fileSystem);

            sut.Location = new FileSystemPath("other");

            oldFile.Received(1).Dispose();
        }

        [Fact]
        public void ShouldOpenNewFileWhenLocationChanges()
        {
            var fileSystem = Substitute.For<IFileSystem>();

            var first = Substitute.For<IFileContents>();
            var second = Substitute.For<IFileContents>();

            fileSystem.GetFileContents(Arg.Any<FileSystemPath>())
                .Returns(first, second);

            var sut = CreateSut(fileSystem: fileSystem);

            var path = new FileSystemPath("new");

            sut.Location = path;

            fileSystem.Received(1).GetFileContents(path);
        }
    }

    public class Dispose
    {
        [Fact]
        public void ShouldFlushBeforeDispose()
        {
            var data = Substitute.For<IEncodableData>();
            data.Encode(Arg.Any<IPersistentDataTranscoder>())
                .Returns("encoded");

            var transcoder = Substitute.For<IPersistentDataTranscoder>();
            transcoder.ElementToBytes("encoded")
                .Returns([]);

            var sut = CreateSut(data: data, transcoder: transcoder);

            sut.Dispose();

            data.Received(1).Encode(transcoder);
        }

        [Fact]
        public void ShouldDisposeCurrentFile()
        {
            var file = Substitute.For<IFileContents>();

            var sut = CreateSut(file: file);

            sut.Dispose();

            file.Received(1).Dispose();
        }

        [Fact]
        public void ShouldRaiseDisposedEvent()
        {
            var sut = CreateSut();

            var raised = false;
            sut.OnDisposed += (_, _) => raised = true;

            sut.Dispose();

            raised.Should().BeTrue();
        }

        [Fact]
        public void ShouldAllowMultipleDisposeCalls()
        {
            var file = Substitute.For<IFileContents>();

            var sut = CreateSut(file: file);

            sut.Dispose();
            sut.Dispose();

            file.Received(1).Dispose();
        }

        [Fact]
        public void ShouldNotFlushAfterDispose()
        {
            var file = Substitute.For<IFileContents>();

            var data = Substitute.For<IEncodableData>();

            var sut = CreateSut(file: file, data: data);

            sut.Dispose();

            sut.SynchronousFlush();

            data.Received(1).Encode(Arg.Any<IPersistentDataTranscoder>());
        }
    }

    private static DataOnDisk<IEncodableData> CreateSut(
        IPersistentDataTranscoder? transcoder = null,
        IFileSystem? fileSystem = null,
        IFileContents? file = null,
        IEncodableData? data = null)
    {
        transcoder ??= Substitute.For<IPersistentDataTranscoder>();

        if (file is null)
        {
            file = Substitute.For<IFileContents>(); 
            file.Contents.Returns([]);
        }

        if (fileSystem is null)
        {
            fileSystem = Substitute.For<IFileSystem>();
            fileSystem.GetFileContents(Arg.Any<FileSystemPath>()).Returns(file);   
        }

        data ??= Substitute.For<IEncodableData>();

        return new DataOnDisk<IEncodableData>(
            new FileSystemPath("test"),
            transcoder,
            fileSystem,
            data);
    }
}
