using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Graph.UnitTests;

public class FileSystemRootsTests
{
    public class AddRoot
    {
        [Fact]
        public void ShouldCreateRootUsingFactory()
        {
            var root = CreateRootFolder();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);
            var sut = CreateRoots(itemFactory: itemFactory);

            sut.AddRoot(root.Path);

            itemFactory.Received(1).CreateRootFolder(root.Path);
        }

        [Fact]
        public void ShouldAddRootToCollection()
        {
            var root = CreateRootFolder();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);

            var sut = CreateRoots(itemFactory: itemFactory);

            sut.AddRoot(root.Path);
            
            sut.Contains(root).Should().BeTrue();
        }

        [Fact]
        public void ShouldAddRootToRepository()
        {
            var root = CreateRootFolder();
            var repository = CreateRepository();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);

            var sut = CreateRoots(repository: repository, itemFactory: itemFactory);

            sut.AddRoot(root.Path);

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), root);
        }

        [Fact]
        public void ShouldReturnCreatedRoot()
        {
            var root = CreateRootFolder();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);

            var sut = CreateRoots(itemFactory: itemFactory);

            var result = sut.AddRoot(root.Path);

            result.Should().BeSameAs(root);
        }
    }

    public class RemoveRootAt
    {
        [Fact]
        public void ShouldReturnFalseWhenRootDoesNotExist()
        {
            var repository = CreateRepository();
            FileSystemPath path = "Root";

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateRoots(repository: repository);

            var result = sut.RemoveRootAt(path, false);

            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldRemoveRootFromRepository()
        {
            var repository = CreateRepository();
            var root = CreateRootFolder();
            var path = root.Path;

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = root;
                return true;
            });

            var sut = CreateRoots(repository: repository);
            sut.AddRoot(path);

            sut.RemoveRootAt(path, false);

            repository.Received(1).Remove(FileSystemGraph.GetTestingToken(), root);
        }

        [Fact]
        public void ShouldNotifyRootItWasRemoved()
        {
            var repository = CreateRepository();
            var root = CreateRootFolder();
            var path = root.Path;

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = root;
                return true;
            });

            var sut = CreateRoots(repository: repository);
            sut.AddRoot(path);

            sut.RemoveRootAt(path, false);

            root.Received(1).OnRemoved(FileSystemGraph.GetTestingToken(), false);
        }

        [Fact]
        public void ShouldRemoveRootFromCollection()
        {
            var repository = CreateRepository();
            var root = CreateRootFolder();
            var path = root.Path;

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = root;
                return true;
            });

            var sut = CreateRoots(repository: repository);
            sut.AddRoot(path);

            sut.RemoveRootAt(path, false);

            sut.Contains(root).Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnRemovedRoot()
        {
            var repository = CreateRepository();
            var root = CreateRootFolder();
            var path = root.Path;

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = root;
                return true;
            });

            var sut = CreateRoots(repository: repository);
            sut.AddRoot(path);

            var result = sut.RemoveRootAt(path, false, out var removedRoot);

            result.Should().BeTrue();
            removedRoot.Should().BeSameAs(root);
        }

        [Fact]
        public void ShouldPassRemoveInfoFilesFlag()
        {
            var repository = CreateRepository();
            var root = CreateRootFolder();
            var path = root.Path;

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = root;
                return true;
            });

            var sut = CreateRoots(repository: repository);
            sut.AddRoot(path);

            sut.RemoveRootAt(path, true);

            root.Received(1).OnRemoved(FileSystemGraph.GetTestingToken(), true);
        }

        [Fact]
        public void ShouldThrowWhenItemAtPathIsNotRootFolder()
        {
            var repository = CreateRepository();
            var item = MockFactory.CreateItem();
            var path = item.Path;

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });

            var sut = CreateRoots(repository: repository);

            var act = () => sut.RemoveRootAt(path, false);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldThrowWhenRootIsNotMutable()
        {
            var repository = CreateRepository();
            var root = Substitute.For<IFileSystemRootFolder>();
            FileSystemPath path = "Root";

            root.Path.Returns(path);

            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = root;
                return true;
            });

            var sut = CreateRoots(repository: repository);

            var act = () => sut.RemoveRootAt(path, false);

            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class CreateDetachedRoot
    {
        [Fact]
        public void ShouldCreateRootUsingFactory()
        {
            var root = CreateRootFolder();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);
            var sut = CreateRoots(itemFactory: itemFactory);

            sut.CreateDetachedRoot(root.Path);

            itemFactory.Received(1).CreateRootFolder(root.Path);
        }

        [Fact]
        public void ShouldAddRootToRepository()
        {
            var root = CreateRootFolder();
            var repository = CreateRepository();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);
            var sut = CreateRoots(repository: repository, itemFactory: itemFactory);

            sut.CreateDetachedRoot(root.Path);

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), root);
        }

        [Fact]
        public void ShouldReturnCreatedRoot()
        {
            var root = CreateRootFolder();
            var itemFactory = CreateFactory();
            itemFactory.CreateRootFolder(root.Path).Returns(root);

            var sut = CreateRoots(itemFactory: itemFactory);

            var result = sut.CreateDetachedRoot(root.Path);

            result.Should().BeSameAs(root);
        }
    }

    private static FileSystemRoots CreateRoots(
        IMutableFileSystemItemRepository? repository = null,
        IFileSystemItemFactory? itemFactory = null)
    {
        return new FileSystemRoots(
            FileSystemGraph.GetTestingToken(),
            repository ?? CreateRepository(),
            itemFactory ?? CreateFactory());
    }

    private static IMutableFileSystemItemRepository CreateRepository()
    {
        return Substitute.For<IMutableFileSystemItemRepository>();
    }

    private static IFileSystemItemFactory CreateFactory()
    {
        return Substitute.For<IFileSystemItemFactory>();
    }

    private static IMutableFileSystemRootFolder CreateRootFolder(
        FileSystemPath? path = null)
    {
        var root = Substitute.For<IMutableFileSystemRootFolder>();

        root.Path.Returns(path ?? MockFactory.CreatePath());

        return root;
    }
}