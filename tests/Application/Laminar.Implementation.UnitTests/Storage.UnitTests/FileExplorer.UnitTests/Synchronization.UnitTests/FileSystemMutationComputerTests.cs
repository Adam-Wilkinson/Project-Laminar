using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Synchronization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Synchronization.UnitTests;

public class FileSystemMutationComputerTests
{
    public class AddEvent
    {
        [Fact]
        public void ShouldIgnoreChangedEvents()
        {
            var sut = CreateComputer();

            sut.AddEvent(FileSystemEvent.Changed(MockFactory.CreatePath()));

            var result = sut.ComputeMutationsAndClear().ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldStoreRelevantEvents()
        {
            var evt = FileSystemEvent.Created(MockFactory.CreatePath());
            var sut = CreateComputer();

            sut.AddEvent(evt);

            var result = sut.ComputeMutationsAndClear().ToList();

            result.Should().ContainSingle().Which.Type.Should().Be(FileSystemGraphMutationType.Creation);
        }
    }

    public class ComputeMutationsAndClear
    {
        [Fact]
        public void ShouldComputeCreationMutation()
        {
            var evt = FileSystemEvent.Created(MockFactory.CreatePath());
            var sut = CreateComputer();

            sut.AddEvent(evt);

            var result = sut.ComputeMutationsAndClear().Single();

            result.Should().Be(
                new FileSystemGraphMutation(
                    FileSystemGraphMutationType.Creation,
                    evt.OldPath,
                    evt.NewPath));
        }

        [Fact]
        public void ShouldComputeDeletionMutation()
        {
            var evt = FileSystemEvent.Deleted(MockFactory.CreatePath());
            var sut = CreateComputer();

            sut.AddEvent(evt);

            var result = sut.ComputeMutationsAndClear().Single();

            result.Should().Be(
                new FileSystemGraphMutation(
                    FileSystemGraphMutationType.Deletion,
                    evt.OldPath,
                    evt.NewPath));
        }

        [Fact]
        public void ShouldComputeRenameMutation()
        {
            var parentPath = MockFactory.CreatePath();
            var evt = FileSystemEvent.Renamed(parentPath.ChildPath("Old"), parentPath.ChildPath("New"));
            var sut = CreateComputer();

            sut.AddEvent(evt);

            var result = sut.ComputeMutationsAndClear().Single();

            result.Should().Be(
                new FileSystemGraphMutation(
                    FileSystemGraphMutationType.Rename,
                    evt.OldPath,
                    evt.NewPath));
        }

        [Fact]
        public void ShouldComputeMoveFromDeletionAndCreationPair()
        {
            const string itemName = "Item Name";
            var deletion = FileSystemEvent.Deleted(MockFactory.CreatePath().ChildPath(itemName));
            var creation = FileSystemEvent.Created(MockFactory.CreatePath().ChildPath(itemName));

            var creationBucket = Substitute.For<IFileSystemEventHashBucket>();
            var deletionBucket = Substitute.For<IFileSystemEventHashBucket>();

            creationBucket.TryGetInfoForPath(deletion.OldPath, out Arg.Any<HashBucketInfo>()).Returns(x =>
            {
                x[1] = new HashBucketInfo
                {
                    State = HashBucketState.Single,
                    Event = creation,
                };

                return true;
            });

            var sut = CreateComputer(
                creationBucket: creationBucket,
                deletionBucket: deletionBucket);

            sut.AddEvent(deletion);
            sut.AddEvent(creation);

            var result = sut.ComputeMutationsAndClear().Single();

            result.Should().Be(new FileSystemGraphMutation(FileSystemGraphMutationType.Move, deletion.OldPath, creation.NewPath));
        }

        [Fact]
        public void ShouldComputeMoveFromCreationAndDeletionPair()
        {
            const string itemName = "Item Name";
            var deletion = FileSystemEvent.Deleted(MockFactory.CreatePath().ChildPath(itemName));
            var creation = FileSystemEvent.Created(MockFactory.CreatePath().ChildPath(itemName));

            var creationBucket = Substitute.For<IFileSystemEventHashBucket>();
            var deletionBucket = Substitute.For<IFileSystemEventHashBucket>();

            deletionBucket.TryGetInfoForPath(creation.NewPath, out Arg.Any<HashBucketInfo>()).Returns(x =>
            {
                x[1] = new HashBucketInfo
                {
                    State = HashBucketState.Single,
                    Event = deletion,
                };

                return true;
            });

            var sut = CreateComputer(creationBucket: creationBucket, deletionBucket: deletionBucket);

            sut.AddEvent(creation);
            sut.AddEvent(deletion);

            var result = sut.ComputeMutationsAndClear().Single();

            result.Should().Be(new FileSystemGraphMutation(FileSystemGraphMutationType.Move, deletion.OldPath, creation.NewPath));
        }

        [Fact]
        public void ShouldIgnoreMoveWhenHashBucketClashes()
        {
            var deletion = FileSystemEvent.Deleted(MockFactory.CreatePath());

            var creationBucket = Substitute.For<IFileSystemEventHashBucket>();

            creationBucket.TryGetInfoForPath(deletion.OldPath, out Arg.Any<HashBucketInfo>()).Returns(x =>
            {
                x[1] = new HashBucketInfo
                {
                    State = HashBucketState.Clash,
                };

                return true;
            });

            var sut = CreateComputer(creationBucket: creationBucket);

            sut.AddEvent(deletion);

            var result = sut.ComputeMutationsAndClear().ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldClearEventsAfterComputing()
        {
            var sut = CreateComputer();

            sut.AddEvent(FileSystemEvent.Created(MockFactory.CreatePath()));

            _ = sut.ComputeMutationsAndClear().ToList();

            var result = sut.ComputeMutationsAndClear().ToList();

            result.Should().BeEmpty();
        }
    }

    private static FileSystemMutationComputer CreateComputer(
        IFileSystemEventHashBucket? creationBucket = null,
        IFileSystemEventHashBucket? deletionBucket = null)
    {
        var factory = Substitute.For<Func<IOutdatedItemsBuffer?, IFileSystemEventHashBucket>>();

        factory(Arg.Any<IOutdatedItemsBuffer?>())
            .Returns(
                creationBucket ?? Substitute.For<IFileSystemEventHashBucket>(),
                deletionBucket ?? Substitute.For<IFileSystemEventHashBucket>());

        return new FileSystemMutationComputer(
            factory,
            Substitute.For<ILogger<FileSystemMutationComputer>>());
    }
}