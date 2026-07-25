using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Graph.UnitTests;

public class FileSystemGraphTests
{
    public class Move
    {
        [Fact]
        public void ShouldReorderChildWhenMovingWithinSameParent()
        {
            var item = CreateItem();
            var parent = CreateFolder(contents: [item]);
            item.ParentFolder.Returns(parent);

            var sut = CreateGraph();

            sut.Move(item, parent, 0);

            parent.Received(1)
                .MoveChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    0,
                    0);
        }

        [Fact]
        public void ShouldRemoveItemFromRepository()
        {
            var repository = CreateRepository();
            var oldParent = CreateFolder();
            var newParent = CreateFolder();
            var item = CreateItem(parent: oldParent);

            var sut = CreateGraph(repository: repository);

            sut.Move(item, newParent, 2);

            repository.Received(1)
                .Remove(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }

        [Fact]
        public void ShouldRemoveItemFromOldParent()
        {
            var oldParent = CreateFolder();
            var newParent = CreateFolder();
            var item = CreateItem(parent: oldParent);

            var sut = CreateGraph();

            sut.Move(item, newParent, 3);

            oldParent.Received(1)
                .RemoveChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }

        [Fact]
        public void ShouldInsertItemIntoNewParent()
        {
            var oldParent = CreateFolder();
            var newParent = CreateFolder();
            var item = CreateItem(parent: oldParent);

            var sut = CreateGraph();

            sut.Move(item, newParent, 5);

            newParent.Received(1)
                .InsertChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    item,
                    5);
        }

        [Fact]
        public void ShouldUpdateParent()
        {
            var oldParent = CreateFolder();
            var newParent = CreateFolder();
            var item = CreateItem(parent: oldParent);

            var sut = CreateGraph();

            sut.Move(item, newParent, 1);

            item.Received(1)
                .SetParentInternal(
                    FileSystemGraph.GetTestingToken(),
                    newParent);
        }

        [Fact]
        public void ShouldAddItemToRepository()
        {
            var repository = CreateRepository();
            var oldParent = CreateFolder();
            var newParent = CreateFolder();
            var item = CreateItem(parent: oldParent);

            var sut = CreateGraph(repository: repository);

            sut.Move(item, newParent, 0);

            repository.Received(1)
                .Add(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }

        [Fact]
        public void ShouldNotUpdateParentWhenMovingWithinSameParent()
        {
            var item = CreateItem();
            var parent = CreateFolder(contents: [item]);
            item.ParentFolder.Returns(parent);

            var repository = CreateRepository();
            var sut = CreateGraph(repository: repository);

            sut.Move(item, parent, 0);

            item.DidNotReceive()
                .SetParentInternal(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemFolder>());

            parent.DidNotReceive()
                .RemoveChildInternal(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>());

            parent.DidNotReceive()
                .InsertChildInternal(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>(),
                    Arg.Any<int>());

            repository.DidNotReceive()
                .Remove(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>());

            repository.DidNotReceive()
                .Add(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>());
        }
    }

    public class Rename
    {
        [Fact]
        public void ShouldRemoveItemFromRepository()
        {
            var repository = CreateRepository();
            var item = CreateItem();

            var sut = CreateGraph(repository: repository);

            sut.Rename(item, "NewName.txt");

            repository.Received(1)
                .Remove(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }

        [Fact]
        public void ShouldRenameItem()
        {
            var item = CreateItem();

            var sut = CreateGraph();

            sut.Rename(item, "NewName.txt");

            item.Received(1)
                .SetNameInternal(
                    FileSystemGraph.GetTestingToken(),
                    "NewName.txt");
        }

        [Fact]
        public void ShouldAddItemBackToRepository()
        {
            var repository = CreateRepository();
            var item = CreateItem();

            var sut = CreateGraph(repository: repository);

            sut.Rename(item, "NewName.txt");

            repository.Received(1)
                .Add(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }
    }
    
    public class Remove
    {
        [Fact]
        public void ShouldRemoveItemFromRepository()
        {
            var repository = CreateRepository();
            var parent = CreateFolder();
            var item = CreateItem(parent: parent);

            var sut = CreateGraph(repository: repository);

            sut.Remove(item);

            repository.Received(1)
                .Remove(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }

        [Fact]
        public void ShouldRemoveItemFromParent()
        {
            var parent = CreateFolder();
            var item = CreateItem(parent: parent);

            var sut = CreateGraph();

            sut.Remove(item);

            parent.Received(1)
                .RemoveChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    item);
        }

        [Fact]
        public void ShouldNotifyItemItWasDeleted()
        {
            var parent = CreateFolder();
            var item = CreateItem(parent: parent);

            var sut = CreateGraph();

            sut.Remove(item);

            item.Received(1).OnDeleted();
        }
    }
    
    public class AddFolder
    {
        [Fact]
        public void ShouldCreateFolderUsingFactory()
        {
            var parent = CreateFolder();
            var createdFolder = CreateFolder();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFolder(parent, "New Folder")
                .Returns(createdFolder);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFolder(parent, 2, "New Folder");

            itemFactory.Received(1)
                .CreateFolder(
                    parent,
                    "New Folder");
        }

        [Fact]
        public void ShouldInsertFolderIntoParent()
        {
            var parent = CreateFolder();
            var createdFolder = CreateFolder();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFolder(parent, "New Folder")
                .Returns(createdFolder);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFolder(parent, 2, "New Folder");

            parent.Received(1)
                .InsertChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    createdFolder,
                    2);
        }

        [Fact]
        public void ShouldAddFolderToRepository()
        {
            var repository = CreateRepository();
            var parent = CreateFolder();
            var createdFolder = CreateFolder();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFolder(parent, "New Folder")
                .Returns(createdFolder);

            var sut = CreateGraph(
                repository: repository,
                itemFactory: itemFactory);

            sut.AddFolder(parent, 2, "New Folder");

            repository.Received(1)
                .Add(
                    FileSystemGraph.GetTestingToken(),
                    createdFolder);
        }

        [Fact]
        public void ShouldReturnCreatedFolder()
        {
            var parent = CreateFolder();
            var createdFolder = CreateFolder();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFolder(parent, "New Folder")
                .Returns(createdFolder);

            var sut = CreateGraph(itemFactory: itemFactory);

            var result = sut.AddFolder(parent, 2, "New Folder");

            result.Should().BeSameAs(createdFolder);
        }
    }
    
    public class AddFile
    {
        [Fact]
        public void ShouldCreateFileUsingFactory()
        {
            var parent = CreateFolder();
            var (file, mutable) = CreateFile();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFile(parent, "File.txt")
                .Returns(file);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFile(parent, 2, "File.txt");

            itemFactory.Received(1)
                .CreateFile(
                    parent,
                    "File.txt");
        }

        [Fact]
        public void ShouldInsertFileIntoParent()
        {
            var parent = CreateFolder();
            var (file, mutable) = CreateFile();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFile(parent, "File.txt")
                .Returns(file);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFile(parent, 2, "File.txt");

            parent.Received(1)
                .InsertChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    mutable,
                    2);
        }

        [Fact]
        public void ShouldAddFileToRepository()
        {
            var repository = CreateRepository();
            var parent = CreateFolder();
            var (file, mutable) = CreateFile();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFile(parent, "File.txt")
                .Returns(file);

            var sut = CreateGraph(
                repository: repository,
                itemFactory: itemFactory);

            sut.AddFile(parent, 2, "File.txt");

            repository.Received(1)
                .Add(
                    FileSystemGraph.GetTestingToken(),
                    mutable);
        }

        [Fact]
        public void ShouldReturnCreatedFile()
        {
            var parent = CreateFolder();
            var (file, mutable) = CreateFile();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFile(parent, "File.txt")
                .Returns(file);

            var sut = CreateGraph(itemFactory: itemFactory);

            var result = sut.AddFile(parent, 2, "File.txt");

            result.Should().BeSameAs(file);
        }
    }

    public class AddFromPersistentData
    {
        [Fact]
        public void ShouldCreateItemUsingFactory()
        {
            var parent = CreateFolder();
            var persistentDictionary = CreatePersistentDictionary();
            var createdItem = CreateItem();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFromPersistentData(parent, persistentDictionary)
                .Returns(createdItem);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFromPersistentData(parent, persistentDictionary);

            itemFactory.Received(1)
                .CreateFromPersistentData(
                    parent,
                    persistentDictionary);
        }

        [Fact]
        public void ShouldAppendItemToParentContents()
        {
            var existingItem = CreateItem();
            var contents = new List<IFileSystemItem> { existingItem };
            var parent = CreateFolder(contents: contents);
            var persistentDictionary = CreatePersistentDictionary();
            var createdItem = CreateItem();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFromPersistentData(parent, persistentDictionary)
                .Returns(createdItem);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFromPersistentData(parent, persistentDictionary);

            parent.Received(1)
                .InsertChildInternal(
                    FileSystemGraph.GetTestingToken(),
                    createdItem,
                    contents.Count);
        }

        [Fact]
        public void ShouldAddItemToRepository()
        {
            var repository = CreateRepository();
            var parent = CreateFolder();
            var persistentDictionary = CreatePersistentDictionary();
            var createdItem = CreateItem();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFromPersistentData(parent, persistentDictionary)
                .Returns(createdItem);

            var sut = CreateGraph(
                repository: repository,
                itemFactory: itemFactory);

            sut.AddFromPersistentData(parent, persistentDictionary);

            repository.Received(1)
                .Add(
                    FileSystemGraph.GetTestingToken(),
                    createdItem);
        }

        [Fact]
        public void ShouldReturnCreatedItem()
        {
            var parent = CreateFolder();
            var persistentDictionary = CreatePersistentDictionary();
            var createdItem = CreateItem();
            var itemFactory = CreateFactory();

            itemFactory
                .CreateFromPersistentData(parent, persistentDictionary)
                .Returns(createdItem);

            var sut = CreateGraph(itemFactory: itemFactory);

            var result = sut.AddFromPersistentData(parent, persistentDictionary);

            result.Should().BeSameAs(createdItem);
        }

        [Fact]
        public void ShouldThrowWhenItemAlreadyExists()
        {
            var repository = CreateRepository();
            var parent = CreateFolder();
            var persistentDictionary = CreatePersistentDictionary();
            var existingItem = CreateItem();
            var childName = "Existing.txt";

            persistentDictionary[IFileSystemItemFactory.PersistenceNameKey]
                .GetValue<string>()
                .Value
                .Returns(childName);

            repository
                .TryGetItem(
                    parent.Path.ChildPath(childName),
                    out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = existingItem;
                    return true;
                });

            var sut = CreateGraph(repository: repository);

            var act = () => sut.AddFromPersistentData(parent, persistentDictionary);

            act.Should()
                .Throw<InvalidOperationException>();
        }
    }

    public class GetOrLoad
    {
        [Fact]
        public async Task ShouldReturnLoadedItem()
        {
            var path = CreatePath();
            var item = CreateItem(path);
            var repository = CreateRepository();
            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = item;
                    return true;
                });

            var sut = CreateGraph(repository: repository);

            var result = await sut.GetOrLoad(path, CancellationToken.None);

            result.Should().BeSameAs(item);
        }

        [Fact]
        public async Task ShouldReturnNullWhenNothingIsLoaded()
        {
            var path = CreatePath();
            var repository = CreateRepository();

            repository.TryGetItem(Arg.Any<FileSystemPath>(), out Arg.Any<IFileSystemItem?>())
                .Returns(false);

            var sut = CreateGraph(repository: repository);

            var result = await sut.GetOrLoad(path, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLoadIntermediateFolders()
        {
            var rootPath = CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var child = CreateItem(childPath);
            var folder = CreateFolder(rootPath);
            
            var repository = CreateRepository();
            
            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = folder;
                    return true;
                });
            
            folder.GetOrLoadContentsAsync().Returns(_ =>
            {
                repository.TryGetItem(childPath, out Arg.Any<IFileSystemItem?>())
                    .Returns(x =>
                    {
                        x[1] = child;
                        return true;
                    });
                return Task.FromResult<IReadOnlyObservableCollection<IFileSystemItem>>(new ObservableCollectionImpl<IFileSystemItem>([child]));
            });

            var sut = CreateGraph(repository: repository);

            var result = await sut.GetOrLoad(childPath, CancellationToken.None);

            result.Should().BeSameAs(child);
            await folder.Received(1).GetOrLoadContentsAsync();
        }

        [Fact]
        public async Task ShouldReturnNullWhenCancelled()
        {
            var rootPath = CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var folder = CreateFolder(rootPath);
            var repository = CreateRepository();

            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = folder;
                    return true;
                });

            var cancellation = new CancellationToken(true);

            var sut = CreateGraph(repository: repository);

            var result = await sut.GetOrLoad(childPath, cancellation);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldThrowWhenLoadedItemIsNotFolder()
        {
            var rootPath = CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var (file, _) = CreateFile(rootPath);

            var repository = CreateRepository();

            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = file;
                    return true;
                });

            var sut = CreateGraph(repository: repository);

            var act = async () =>
                await sut.GetOrLoad(childPath, CancellationToken.None);

            await act.Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ShouldThrowWhenFolderLoadDoesNotPopulateRepository()
        {
            var rootPath = CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var folder = CreateFolder(rootPath);

            var repository = CreateRepository();

            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>())
                .Returns(x =>
                {
                    x[1] = folder;
                    return true;
                });

            repository.TryGetItem(childPath, out Arg.Any<IFileSystemItem?>())
                .Returns(false);

            var sut = CreateGraph(repository: repository);

            var act = async () =>
                await sut.GetOrLoad(childPath, CancellationToken.None);

            await act.Should()
                .ThrowAsync<InvalidOperationException>();
        }
    }
    
    private static FileSystemGraph CreateGraph(
        IMutableFileSystemItemRepository? repository = null,
        IFileSystemItemFactory? itemFactory = null)
    {
        return new FileSystemGraph(
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

    private static (IFileSystemFile file, IMutableFileSystemItem mutable) CreateFile(
        FileSystemPath? path = null,
        IMutableFileSystemFolder? parent = null)
    {
        var file = Substitute.For<IMutableFileSystemItem, IFileSystemFile>();

        path ??= CreatePath();

        file.Path.Returns(path.Value);
        file.ParentFolder.Returns(parent);

        return ((IFileSystemFile)file, (IMutableFileSystemItem)file);
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

    private static FileSystemPath CreatePath(
        FileSystemPath? parent = null,
        string? name = null)
    {
        parent ??= "Parent";
        name ??= "Child";
        var path = parent.Value.ChildPath(name);
        
        return path;
    }

    private static IPersistentDictionary CreatePersistentDictionary(
        string name = "Item",
        bool isFolder = false)
    {
        var dictionary = Substitute.For<IPersistentDictionary>();

        var namePoint = CreateDataPoint(name);
        var folderPoint = CreateDataPoint(isFolder);

        dictionary[IFileSystemItemFactory.PersistenceNameKey].Returns(namePoint);
        dictionary[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(folderPoint);

        return dictionary;
    }

    private static IPersistentDataPoint CreateDataPoint<T>(T value)
        where T : notnull
    {
        var point = Substitute.For<IPersistentDataPoint>();
        var persistentValue = Substitute.For<IPersistentValue<T>>();

        persistentValue.Value.Returns(value);
        point.GetValue<T>().Returns(persistentValue);

        return point;
    }
}