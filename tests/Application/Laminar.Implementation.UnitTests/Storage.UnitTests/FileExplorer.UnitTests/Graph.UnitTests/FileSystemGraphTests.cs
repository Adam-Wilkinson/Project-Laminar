using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
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
            var item = MockFactory.CreateItem();
            var parent = MockFactory.CreateFolder(contents: [item]);
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
            var oldParent = MockFactory.CreateFolder();
            var newParent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: oldParent);

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
            var oldParent = MockFactory.CreateFolder();
            var newParent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: oldParent);

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
            var oldParent = MockFactory.CreateFolder();
            var newParent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: oldParent);

            var sut = CreateGraph();

            sut.Move(item, newParent, 5);

            newParent.Received(1).InsertChildInternal(FileSystemGraph.GetTestingToken(), item, 5);
        }

        [Fact]
        public void ShouldUpdateParent()
        {
            var oldParent = MockFactory.CreateFolder();
            var newParent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: oldParent);

            var sut = CreateGraph();

            sut.Move(item, newParent, 1);

            item.Received(1).SetParentInternal(FileSystemGraph.GetTestingToken(), newParent);
        }

        [Fact]
        public void ShouldAddItemToRepository()
        {
            var repository = CreateRepository();
            var oldParent = MockFactory.CreateFolder();
            var newParent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: oldParent);

            var sut = CreateGraph(repository: repository);

            sut.Move(item, newParent, 0);

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), item);
        }

        [Fact]
        public void ShouldNotUpdateParentWhenMovingWithinSameParent()
        {
            var item = MockFactory.CreateItem();
            var parent = MockFactory.CreateFolder(contents: [item]);
            item.ParentFolder.Returns(parent);

            var repository = CreateRepository();
            var sut = CreateGraph(repository: repository);

            sut.Move(item, parent, 0);

            item.DidNotReceive().SetParentInternal(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemFolder>());

            parent.DidNotReceive().RemoveChildInternal(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>());

            parent.DidNotReceive().InsertChildInternal(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>(),
                    Arg.Any<int>());

            repository.DidNotReceive().Remove(
                    Arg.Any<FileSystemGraph.MutationToken>(),
                    Arg.Any<IFileSystemItem>());

            repository.DidNotReceive().Add(
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
            var item = MockFactory.CreateItem();

            var sut = CreateGraph(repository: repository);

            sut.Rename(item, "NewName.txt");

            repository.Received(1).Remove(FileSystemGraph.GetTestingToken(), item);
        }

        [Fact]
        public void ShouldRenameItem()
        {
            var item = MockFactory.CreateItem();

            var sut = CreateGraph();

            sut.Rename(item, "NewName.txt");

            item.Received(1).SetNameInternal(FileSystemGraph.GetTestingToken(), "NewName.txt");
        }

        [Fact]
        public void ShouldAddItemBackToRepository()
        {
            var repository = CreateRepository();
            var item = MockFactory.CreateItem();

            var sut = CreateGraph(repository: repository);

            sut.Rename(item, "NewName.txt");

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), item);
        }
    }
    
    public class Remove
    {
        [Fact]
        public void ShouldRemoveItemFromRepository()
        {
            var repository = CreateRepository();
            var parent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: parent);

            var sut = CreateGraph(repository: repository);

            sut.Remove(item);

            repository.Received(1).Remove(FileSystemGraph.GetTestingToken(), item);
        }

        [Fact]
        public void ShouldRemoveItemFromParent()
        {
            var parent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: parent);

            var sut = CreateGraph();

            sut.Remove(item);

            parent.Received(1).RemoveChildInternal(FileSystemGraph.GetTestingToken(), item);
        }

        [Fact]
        public void ShouldNotifyItemItWasDeleted()
        {
            var parent = MockFactory.CreateFolder();
            var item = MockFactory.CreateItem(parent: parent);

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
            const string folderName = "New Folder";
            var parent = MockFactory.CreateFolder();
            var createdFolder = MockFactory.CreateFolder(parent.Path.ChildPath(folderName));
            var itemFactory = CreateFactory();

            itemFactory.CreateFolder(parent, folderName).Returns(createdFolder);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFolder(parent, 2, folderName);

            itemFactory.Received(1).CreateFolder(parent, folderName);
        }

        [Fact]
        public void ShouldInsertFolderIntoParent()
        {
            const string folderName = "New Folder";
            var parent = MockFactory.CreateFolder();
            var createdFolder = MockFactory.CreateFolder(MockFactory.CreatePath(folderName));
            var itemFactory = CreateFactory();

            itemFactory.CreateFolder(parent, folderName).Returns(createdFolder);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFolder(parent, 2, folderName);

            parent.Received(1).InsertChildInternal(FileSystemGraph.GetTestingToken(), createdFolder, 2);
        }

        [Fact]
        public void ShouldAddFolderToRepository()
        {
            const string folderName = "New Folder";
            var repository = CreateRepository();
            var parent = MockFactory.CreateFolder();
            var createdFolder = MockFactory.CreateFolder();
            var itemFactory = CreateFactory();

            itemFactory.CreateFolder(parent, folderName).Returns(createdFolder);

            var sut = CreateGraph(repository: repository, itemFactory: itemFactory);

            sut.AddFolder(parent, 2, folderName);

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), createdFolder);
        }

        [Fact]
        public void ShouldReturnCreatedFolder()
        {
            const string folderName = "New Folder";
            var parent = MockFactory.CreateFolder();
            var createdFolder = MockFactory.CreateFolder();
            var itemFactory = CreateFactory();

            itemFactory.CreateFolder(parent, folderName).Returns(createdFolder);

            var sut = CreateGraph(itemFactory: itemFactory);

            var result = sut.AddFolder(parent, 2, folderName);

            result.Should().BeSameAs(createdFolder);
        }
    }
    
    public class AddFile
    {
        private const string FileName = "File.txt";
            
        [Fact]
        public void ShouldCreateFileUsingFactory()
        {
            var parent = MockFactory.CreateFolder();
            var (file, mutable) = MockFactory.CreateFile();
            var itemFactory = CreateFactory();

            itemFactory.CreateFile(parent, FileName).Returns(file);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFile(parent, 2, FileName);

            itemFactory.Received(1).CreateFile(parent, FileName);
        }

        [Fact]
        public void ShouldInsertFileIntoParent()
        {
            var parent = MockFactory.CreateFolder();
            var (file, mutable) = MockFactory.CreateFile();
            var itemFactory = CreateFactory();

            itemFactory.CreateFile(parent, FileName).Returns(file);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFile(parent, 2, FileName);

            parent.Received(1).InsertChildInternal(FileSystemGraph.GetTestingToken(), mutable, 2);
        }

        [Fact]
        public void ShouldAddFileToRepository()
        {
            var repository = CreateRepository();
            var parent = MockFactory.CreateFolder();
            var (file, mutable) = MockFactory.CreateFile();
            var itemFactory = CreateFactory();

            itemFactory.CreateFile(parent, FileName).Returns(file);

            var sut = CreateGraph(repository: repository, itemFactory: itemFactory);

            sut.AddFile(parent, 2, FileName);

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), mutable);
        }

        [Fact]
        public void ShouldReturnCreatedFile()
        {
            var parent = MockFactory.CreateFolder();
            var (file, mutable) = MockFactory.CreateFile();
            var itemFactory = CreateFactory();

            itemFactory.CreateFile(parent, FileName).Returns(file);

            var sut = CreateGraph(itemFactory: itemFactory);

            var result = sut.AddFile(parent, 2, FileName);

            result.Should().BeSameAs(file);
        }
    }

    public class AddFromPersistentData
    {
        [Fact]
        public void ShouldCreateItemUsingFactory()
        {
            var parent = MockFactory.CreateFolder();
            var persistentDictionary = MockFactory.CreateItemData();
            var createdItem = MockFactory.CreateItem();
            var itemFactory = CreateFactory();

            itemFactory.CreateFromPersistentData(parent, persistentDictionary).Returns(createdItem);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFromPersistentData(parent, persistentDictionary);

            itemFactory.Received(1).CreateFromPersistentData(parent, persistentDictionary);
        }

        [Fact]
        public void ShouldAppendItemToParentContents()
        {
            var existingItem = MockFactory.CreateItem();
            var contents = new List<IFileSystemItem> { existingItem };
            var parent = MockFactory.CreateFolder(contents: contents);
            var persistentDictionary = MockFactory.CreateItemData();
            var createdItem = MockFactory.CreateItem();
            var itemFactory = CreateFactory();

            itemFactory.CreateFromPersistentData(parent, persistentDictionary).Returns(createdItem);

            var sut = CreateGraph(itemFactory: itemFactory);

            sut.AddFromPersistentData(parent, persistentDictionary);

            parent.Received(1).InsertChildInternal(FileSystemGraph.GetTestingToken(), createdItem, contents.Count);
        }

        [Fact]
        public void ShouldAddItemToRepository()
        {
            var repository = CreateRepository();
            var parent = MockFactory.CreateFolder();
            var persistentDictionary = MockFactory.CreateItemData();
            var createdItem = MockFactory.CreateItem();
            var itemFactory = CreateFactory();

            itemFactory.CreateFromPersistentData(parent, persistentDictionary).Returns(createdItem);

            var sut = CreateGraph(repository: repository, itemFactory: itemFactory);

            sut.AddFromPersistentData(parent, persistentDictionary);

            repository.Received(1).Add(FileSystemGraph.GetTestingToken(), createdItem);
        }

        [Fact]
        public void ShouldReturnCreatedItem()
        {
            var parent = MockFactory.CreateFolder();
            var persistentDictionary = MockFactory.CreateItemData();
            var createdItem = MockFactory.CreateItem();
            var itemFactory = CreateFactory();

            itemFactory.CreateFromPersistentData(parent, persistentDictionary).Returns(createdItem);

            var sut = CreateGraph(itemFactory: itemFactory);

            var result = sut.AddFromPersistentData(parent, persistentDictionary);

            result.Should().BeSameAs(createdItem);
        }

        [Fact]
        public void ShouldThrowWhenItemAlreadyExists()
        {
            var repository = CreateRepository();
            var parent = MockFactory.CreateFolder();
            var persistentDictionary = MockFactory.CreateItemData();
            var existingItem = MockFactory.CreateItem();
            var childName = "Existing.txt";

            persistentDictionary[IFileSystemItemFactory.PersistenceNameKey].GetValue<string>().Value.Returns(childName);

            repository.TryGetItem(parent.Path.ChildPath(childName), out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = existingItem;
                return true;
            });

            var sut = CreateGraph(repository: repository);

            var act = () => sut.AddFromPersistentData(parent, persistentDictionary);

            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class GetOrLoad
    {
        [Fact]
        public async Task ShouldReturnLoadedItem()
        {
            var path = MockFactory.CreatePath();
            var item = MockFactory.CreateItem(path);
            var repository = CreateRepository();
            repository.TryGetItem(path, out Arg.Any<IFileSystemItem?>()).Returns(x =>
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
            var path = MockFactory.CreatePath();
            var repository = CreateRepository();

            repository.TryGetItem(Arg.Any<FileSystemPath>(), out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateGraph(repository: repository);

            var result = await sut.GetOrLoad(path, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ShouldLoadIntermediateFolders()
        {
            var rootPath = MockFactory.CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var child = MockFactory.CreateItem(childPath);
            var folder = MockFactory.CreateFolder(rootPath);
            
            var repository = CreateRepository();
            
            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = folder;
                return true;
            });
            
            folder.GetOrLoadContentsAsync().Returns(_ =>
            {
                repository.TryGetItem(childPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
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
            var rootPath = MockFactory.CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var folder = MockFactory.CreateFolder(rootPath);
            var repository = CreateRepository();

            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
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
            var rootPath = MockFactory.CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var (file, _) = MockFactory.CreateFile(rootPath);

            var repository = CreateRepository();

            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = file;
                return true;
            });

            var sut = CreateGraph(repository: repository);

            var act = async () => await sut.GetOrLoad(childPath, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ShouldThrowWhenFolderLoadDoesNotPopulateRepository()
        {
            var rootPath = MockFactory.CreatePath();
            var childPath = rootPath.ChildPath("Child");
            var folder = MockFactory.CreateFolder(rootPath);

            var repository = CreateRepository();

            repository.TryGetItem(rootPath, out Arg.Any<IFileSystemItem?>()).Returns(x =>
            {
                x[1] = folder;
                return true;
            });

            repository.TryGetItem(childPath, out Arg.Any<IFileSystemItem?>()).Returns(false);

            var sut = CreateGraph(repository: repository);

            var act = async () => await sut.GetOrLoad(childPath, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
    
    private static FileSystemGraph CreateGraph(IMutableFileSystemItemRepository? repository = null,
        IFileSystemItemFactory? itemFactory = null) 
        => new(repository ?? CreateRepository(), itemFactory ?? CreateFactory());

    private static IMutableFileSystemItemRepository CreateRepository() 
        => Substitute.For<IMutableFileSystemItemRepository>();

    private static IFileSystemItemFactory CreateFactory() 
        => Substitute.For<IFileSystemItemFactory>();
}