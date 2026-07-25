using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Notification.Collections;
using Laminar.Implementation.Storage.FileExplorer.Synchronization;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Synchronization.UnitTests;

public class FileSystemItemHasherTests
{
    public class TryHashItem
    {
        [Fact]
        public void ShouldHashFile()
        {
            var (file, _) = MockFactory.CreateFile();
            file.SizeOnDisk.Returns(100);

            var sut = CreateHasher();

            var result = sut.TryHashItem(file, null, out var hash);

            result.Should().BeTrue();
            hash.Should().NotBe(-1);
        }

        [Fact]
        public void ShouldUsePathOverrideWhenHashingFile()
        {
            var (file, _) = MockFactory.CreateFile();
            var originalPath = file.Path;
            var overridePath = MockFactory.CreatePath();

            file.SizeOnDisk.Returns(100);
            file.Path.Returns(originalPath);

            var sut = CreateHasher();

            sut.TryHashItem(file, null, out var originalHash);
            sut.TryHashItem(file, overridePath, out var overriddenHash);

            overriddenHash.Should().NotBe(originalHash);
        }

        [Fact]
        public void ShouldChangeHashWhenFileSizeChanges()
        {
            var (first, _) = MockFactory.CreateFile();
            var (second, _) = MockFactory.CreateFile();

            first.SizeOnDisk.Returns(100);
            second.SizeOnDisk.Returns(200);

            var sut = CreateHasher();

            sut.TryHashItem(first, null, out var firstHash);
            sut.TryHashItem(second, null, out var secondHash);

            secondHash.Should().NotBe(firstHash);
        }

        [Fact]
        public void ShouldHashLoadedFolder()
        {
            var folder = MockFactory.CreateFolder();

            var sut = CreateHasher();

            var result = sut.TryHashItem(folder, null, out var hash);

            result.Should().BeTrue();
            hash.Should().NotBe(-1);
        }

        [Fact]
        public void ShouldNotHashUnloadedFolder()
        {
            var folder = MockFactory.CreateFolder();
            folder.Contents.Returns((IReadOnlyObservableCollection<IFileSystemItem>?)null);

            var sut = CreateHasher();

            var result = sut.TryHashItem(folder, null, out var hash);

            result.Should().BeFalse();
            hash.Should().Be(-1);
        }

        [Fact]
        public void ShouldChangeHashWhenFolderContentsChange()
        {
            var (child, _) = MockFactory.CreateFile();
            child.SizeOnDisk.Returns(100);

            var firstFolder = MockFactory.CreateFolder(contents: []);
            var secondFolder = MockFactory.CreateFolder(contents: [child]);

            var sut = CreateHasher();

            sut.TryHashItem(firstFolder, null, out var firstHash);
            sut.TryHashItem(secondFolder, null, out var secondHash);

            secondHash.Should().NotBe(firstHash);
        }

        [Fact]
        public void ShouldNotDependOnFolderChildOrder()
        {
            var (first, _) = MockFactory.CreateFile();
            first.SizeOnDisk.Returns(100);

            var (second, _) = MockFactory.CreateFile();
            second.SizeOnDisk.Returns(200);

            var folderPath = MockFactory.CreatePath();
            var firstFolder = MockFactory.CreateFolder(path: folderPath, contents: [first, second]);
            var secondFolder = MockFactory.CreateFolder(path: folderPath, contents: [second, first]);

            var sut = CreateHasher();

            sut.TryHashItem(firstFolder, null, out var firstHash);
            sut.TryHashItem(secondFolder, null, out var secondHash);

            secondHash.Should().Be(firstHash);
        }
    }

    public class HashFromPath
    {
        [Fact]
        public void ShouldHashFileFromPath()
        {
            var path = MockFactory.CreatePath();
            var fileSystem = CreateFileSystem();

            fileSystem.IsDirectory(path).Returns(false);

            fileSystem.GetFileSize(path).Returns(100);

            var sut = CreateHasher(fileSystem);

            var hash = sut.HashFromPath(path);

            hash.Should().NotBe(-1);
        }

        [Fact]
        public void ShouldHashFolderFromPath()
        {
            var path = MockFactory.CreatePath();
            var childPath = MockFactory.CreatePath(path);
            var fileSystem = CreateFileSystem();
            fileSystem.IsDirectory(path).Returns(true);
            fileSystem.EnumerateChildren(path).Returns([childPath]);
            fileSystem.IsDirectory(childPath).Returns(false);
            fileSystem.GetFileSize(childPath).Returns(100);

            var sut = CreateHasher(fileSystem);

            var hash = sut.HashFromPath(path);

            hash.Should().NotBe(-1);
        }

        [Fact]
        public void ShouldUseFileMetadataWhenHashingFromPath()
        {
            var path = MockFactory.CreatePath();
            var fileSystem = CreateFileSystem();

            fileSystem.IsDirectory(path).Returns(false);

            fileSystem.GetFileSize(path).Returns(100);

            var sut = CreateHasher(fileSystem);

            var firstHash = sut.HashFromPath(path);

            fileSystem.GetFileSize(path).Returns(200);

            var secondHash = sut.HashFromPath(path);

            secondHash.Should().NotBe(firstHash);
        }
    }

    private static FileSystemItemHasher CreateHasher(
        IFileSystem? fileSystem = null)
    {
        return new FileSystemItemHasher(
            fileSystem ?? CreateFileSystem());
    }

    private static IFileSystem CreateFileSystem()
    {
        return Substitute.For<IFileSystem>();
    }
}