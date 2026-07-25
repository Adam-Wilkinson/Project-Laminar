using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Synchronization;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Synchronization.UnitTests;

public class FileSystemEventHashBucketTests
{
    public class TryGetInfoForPath
    {
        [Fact]
        public void ShouldReturnFalseWhenPathMissing()
        {
            var sut = CreateBucket();

            var result = sut.TryGetInfoForPath(null, out _);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnAddedEventInfo()
        {
            var path = MockFactory.CreatePath();
            var fileSystemEvent = CreateEvent(path);
            var hasher = CreateHasher();

            hasher.TryHashItem(Arg.Any<IFileSystemItem>(), Arg.Any<FileSystemPath>(),out Arg.Any<int>())
            .Returns(x =>
            {
                x[2] = 123;
                return true;
            });

            var item = MockFactory.CreateItem();
            var repository = CreateRepository();

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });
            
            var sut = CreateBucket(hasher: hasher, repository: repository);

            sut.AddEvent(fileSystemEvent);

            var result = sut.TryGetInfoForPath(path, out var info);

            result.Should().BeTrue();
            info.Event.Should().Be(fileSystemEvent);
        }
    }

    public class AddEvent
    {
        [Fact]
        public void ShouldIgnoreEventWithoutPath()
        {
            var fileSystemEvent = CreateEvent(null);
            var sut = CreateBucket();

            sut.AddEvent(fileSystemEvent);

            sut.TryGetInfoForPath(null, out _).Should().BeFalse();
        }

        [Fact]
        public void ShouldAddSingleEvent()
        {
            var path = MockFactory.CreatePath();
            var fileSystemEvent = CreateEvent(path);
            var sut = CreateBucketWithHash(path, 123);

            sut.AddEvent(fileSystemEvent);

            sut.TryGetInfoForPath(path, out var info).Should().BeTrue();

            info.Event.Should().Be(fileSystemEvent);
            info.State.Should().Be(HashBucketState.Single);
            info.Hash.Should().Be(123);
        }

        [Fact]
        public void ShouldMarkSameHashAsClash()
        {
            var firstPath = MockFactory.CreatePath();
            var secondPath = MockFactory.CreatePath();

            var firstEvent = CreateEvent(firstPath);
            var secondEvent = CreateEvent(secondPath);
            
            var hasher = Substitute.For<IFileSystemItemHasher>();

            var firstItem = MockFactory.CreateItem(firstPath);
            var secondItem = MockFactory.CreateItem(secondPath);

            var repository = CreateRepository();

            repository.TryGetItem(firstPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = firstItem;
                return true;
            });

            repository.TryGetItem(secondPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = secondItem;
                return true;
            });

            hasher.TryHashItem(firstItem, firstPath, out Arg.Any<int>()).Returns(x =>
            {
                x[2] = 123;
                return true;
            });

            hasher.TryHashItem(secondItem, secondPath, out Arg.Any<int>()).Returns(x =>
            {
                x[2] = 123;
                return true;
            });

            var sut = CreateBucket(hasher: hasher, repository: repository);

            sut.AddEvent(firstEvent);
            sut.AddEvent(secondEvent);

            sut.TryGetInfoForPath(firstPath, out var info).Should().BeTrue();

            info.State.Should().Be(HashBucketState.Clash);
            info.Event.Should().BeNull();
        }

        [Fact]
        public void ShouldUseOutdatedItemBeforeRepositoryItem()
        {
            var path = MockFactory.CreatePath();
            var outdatedItem = MockFactory.CreateItem();
            var repositoryItem = MockFactory.CreateItem();

            var outdated = Substitute.For<IOutdatedItemsBuffer>();
            outdated.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = outdatedItem;
                return true;
            });

            var repository = CreateRepository();
            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = repositoryItem;
                return true;
            });

            var hasher = CreateHasher();

            hasher.TryHashItem(outdatedItem, path,out Arg.Any<int>()).Returns(x =>
            {
                x[2] = 123;
                return true;
            });

            var sut = CreateBucket(outdatedFileLocations: outdated, repository: repository, hasher: hasher);

            sut.AddEvent(CreateEvent(path));

            hasher.Received(1).TryHashItem(outdatedItem, path, out Arg.Any<int>());

            hasher.DidNotReceive().TryHashItem(repositoryItem, path, out Arg.Any<int>());
        }
    }

    public class TryHashByPath
    {
        [Fact]
        public void ShouldHashFromRepositoryItem()
        {
            var path = MockFactory.CreatePath();
            var item = MockFactory.CreateItem();
            var repository = CreateRepository();
            var hasher = CreateHasher();

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });

            var sut = CreateBucket(repository: repository, hasher: hasher);

            sut.AddEvent(CreateEvent(path));

            hasher.Received(1).TryHashItem(item, path, out Arg.Any<int>());
        }

        [Fact]
        public void ShouldHashFromFilesystemWhenRepositoryMissing()
        {
            var path = MockFactory.CreatePath();
            var repository = CreateRepository();
            var fileSystem = CreateFileSystem();
            var hasher = CreateHasher();

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(false);

            fileSystem.Exists(path).Returns(true);

            var sut = CreateBucket(repository: repository, fileSystem: fileSystem, hasher: hasher);

            sut.AddEvent(CreateEvent(path));

            hasher.Received(1).HashFromPath(path);
        }

        [Fact]
        public void ShouldIgnoreMissingFilesystemItem()
        {
            var path = MockFactory.CreatePath();
            var repository = CreateRepository();
            var fileSystem = CreateFileSystem();

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(false);

            fileSystem.Exists(path).Returns(false);

            var sut = CreateBucket(repository: repository, fileSystem: fileSystem);

            sut.AddEvent(CreateEvent(path));

            sut.TryGetInfoForPath(path, out _).Should().BeFalse();
        }
    }

    private static FileSystemEventHashBucket CreateBucket(
        IOutdatedItemsBuffer? outdatedFileLocations = null,
        IFileSystemItemHasher? hasher = null,
        IFileSystem? fileSystem = null,
        IFileSystemItemRepository? repository = null)
    {
        return new FileSystemEventHashBucket(
            outdatedFileLocations,
            hasher ?? CreateHasher(),
            fileSystem ?? CreateFileSystem(),
            repository ?? CreateRepository());
    }

    private static FileSystemEventHashBucket CreateBucketWithHash(FileSystemPath path, int hash)
    {
        var item = MockFactory.CreateItem(path);
        var repository = CreateRepository();
        var hasher = CreateHasher();

        repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
        {
            x[1] = item;
            return true;
        });

        hasher.TryHashItem(item, path, out Arg.Any<int>()).Returns(x =>
        {
            x[2] = hash;
            return true;
        });

        return CreateBucket(hasher: hasher, repository: repository);
    }

    private static IFileSystemItemHasher CreateHasher()
        => Substitute.For<IFileSystemItemHasher>();

    private static IFileSystem CreateFileSystem()
        => Substitute.For<IFileSystem>();

    private static IFileSystemItemRepository CreateRepository()
        => Substitute.For<IFileSystemItemRepository>();

    private static FileSystemEvent CreateEvent(FileSystemPath? path) => FileSystemEvent.Created(path ?? MockFactory.CreatePath());
}