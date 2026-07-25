using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Graph.UnitTests;

public class GraphMutationApplierTests
{
    public class ApplyMove
    {
        [Fact]
        public void ShouldMoveItemWhenNewParentExists()
        {
            var oldPath = CreatePath("Old");
            var newPath = CreatePath("New");
            var item = CreateItem(oldPath);
            var newParent = CreateFolder(oldPath.Parent);
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Move, oldPath, newPath);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(oldPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = newParent;
                return true;
            });

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.Received(1).Move(item, newParent, 0);
        }

        [Fact]
        public void ShouldNotMoveItemWhenNewParentCannotBeFound()
        {
            var oldPath = CreatePath(name: "Old");
            var newPath = CreatePath(name: "New");
            var item = CreateItem();
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Move, oldPath, newPath);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(oldPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.DidNotReceive().Move(Arg.Any<IFileSystemItem>(), Arg.Any<IFileSystemFolder>(), Arg.Any<int>());
        }

        [Fact]
        public void ShouldNotMoveItemWhenAlreadyInNewParentContents()
        {
            var oldPath = CreatePath("Old");
            var newPath = CreatePath("New");
            var item = CreateItem();
            var newParent = CreateFolder(contents: [item]);

            var mutation = new FileSystemGraphMutation(
                FileSystemGraphMutationType.Move,
                oldPath,
                newPath);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(oldPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = newParent;
                return true;
            });

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.DidNotReceive().Move(Arg.Any<IFileSystemItem>(), Arg.Any<IFileSystemFolder>(), Arg.Any<int>());
        }
    }

    public class ApplyCreation
    {
        [Fact]
        public void ShouldAddFolderWhenPathIsDirectory()
        {
            var newPath = CreatePath("New");
            var parent = CreateFolder();
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Creation, null, newPath);

            var repository = CreateRepository();
            var fileSystem = CreateFileSystem();
            var graph = CreateGraph();

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = parent;
                    return true;
                });
            
            fileSystem.IsDirectory(newPath)
                .Returns(true);

            var sut = CreateApplier(
                fileSystem: fileSystem,
                repository: repository);

            sut.Apply(mutation, graph);

            graph.Received(1).AddFolder(parent, 0, newPath.NameAndExtension);
        }

        [Fact]
        public void ShouldAddFileWhenPathIsNotDirectory()
        {
            var newPath = CreatePath();
            var parent = CreateFolder();
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Creation, null, newPath);

            var repository = CreateRepository();
            var fileSystem = CreateFileSystem();
            var graph = CreateGraph();

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = parent;
                return true;
            });
            
            fileSystem.IsDirectory(newPath).Returns(false);

            var sut = CreateApplier(fileSystem: fileSystem, repository: repository);

            sut.Apply(mutation, graph);

            graph.Received(1).AddFile(parent, 0, newPath.NameAndExtension);
        }

        [Fact]
        public void ShouldNotCreateWhenParentCannotBeFound()
        {
            var newPath = CreatePath("New");
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Creation, null, newPath);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.DidNotReceive().AddFile(Arg.Any<IFileSystemFolder>(), Arg.Any<int>(), Arg.Any<string>());

            graph.DidNotReceive().AddFolder(Arg.Any<IFileSystemFolder>(), Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public void ShouldNotCreateWhenItemAlreadyExists()
        {
            var newPath = CreatePath("New");
            var existingItem = CreateItem(newPath);
            var parent = CreateFolder(contents: [existingItem]);

            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Creation, null, newPath);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(newPath.Parent!.Value, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = parent;
                return true;
            });
            
            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.DidNotReceive().AddFile(Arg.Any<IFileSystemFolder>(), Arg.Any<int>(), Arg.Any<string>());
            graph.DidNotReceive().AddFolder(Arg.Any<IFileSystemFolder>(), Arg.Any<int>(), Arg.Any<string>());
        }
    }

    public class ApplyDeletion
    {
        [Fact]
        public void ShouldRemoveExistingItem()
        {
            var oldPath = CreatePath("Old");
            var item = CreateItem();

            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Deletion, oldPath, null);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(oldPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = item;
                return true;
            });

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.Received(1).Remove(item);
        }

        [Fact]
        public void ShouldIgnoreDeletionWhenItemDoesNotExist()
        {
            var oldPath = CreatePath("Old");

            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Deletion, oldPath, null);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(oldPath, out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.DidNotReceive().Remove(Arg.Any<IFileSystemItem>());
        }
    }

    public class ApplyRename
    {
        [Fact]
        public void ShouldRenameExistingItem()
        {
            var oldPath = CreatePath("Old");
            var newPath = CreatePath("New");
            var item = CreateItem();

            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Rename, oldPath, newPath);

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(oldPath, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = item;
                    return true;
                });

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.Received(1).Rename(item, newPath.NameAndExtension);
        }

        [Fact]
        public void ShouldIgnoreRenameWhenItemDoesNotExist()
        {
            var mutation = new FileSystemGraphMutation(FileSystemGraphMutationType.Rename, "Old", "New");

            var repository = CreateRepository();
            var graph = CreateGraph();

            repository.TryGetItem(mutation.OldPath!.Value, out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateApplier(repository: repository);

            sut.Apply(mutation, graph);

            graph.DidNotReceive().Rename(Arg.Any<IFileSystemItem>(), Arg.Any<string>());
        }
    }

    private static GraphMutationApplier CreateApplier(
        IFileSystem? fileSystem = null,
        IFileSystemItemRepository? repository = null,
        ILogger<GraphMutationApplier>? logger = null)
    {
        return new GraphMutationApplier(
            fileSystem ?? CreateFileSystem(),
            repository ?? CreateRepository(),
            logger ?? Substitute.For<ILogger<GraphMutationApplier>>());
    }

    private static IFileSystem CreateFileSystem()
    {
        return Substitute.For<IFileSystem>();
    }

    private static IFileSystemItemRepository CreateRepository()
    {
        return Substitute.For<IFileSystemItemRepository>();
    }

    private static IFileSystemGraph CreateGraph()
    {
        return Substitute.For<IFileSystemGraph>();
    }
    
    

    private static IMutableFileSystemItem CreateItem(
        FileSystemPath? path = null,
        IMutableFileSystemFolder? parent = null)
    {
        var item = Substitute.For<IMutableFileSystemItem>();

        path ??= CreatePath();

        item.Path.Returns(path.Value);
        item.ParentFolder.Returns(parent);

        return item;
    }
    
    private static IMutableFileSystemFolder CreateFolder(
        FileSystemPath? path = null,
        IMutableFileSystemFolder? parent = null,
        IReadOnlyList<IFileSystemItem>? contents = null)
    {
        var folder = Substitute.For<IMutableFileSystemFolder>();

        path ??= CreatePath();
        contents ??= [];
        
        IReadOnlyObservableCollection<IFileSystemItem> contentsObservable = new ObservableCollectionImpl<IFileSystemItem>(contents);

        folder.Path.Returns(path.Value);
        folder.ParentFolder.Returns(parent);
        folder.GetOrLoadContents().Returns(contentsObservable);
        folder.GetOrLoadContentsAsync().Returns(Task.FromResult(contentsObservable));

        return folder;
    }
    
    private static FileSystemPath CreatePath(
        FileSystemPath? parent = null,
        string? name = null)
    {
        parent ??= "Parent";
        name ??= "Child";
        var path = parent.Value.ChildPath(name);
        
        return path;
    }
}