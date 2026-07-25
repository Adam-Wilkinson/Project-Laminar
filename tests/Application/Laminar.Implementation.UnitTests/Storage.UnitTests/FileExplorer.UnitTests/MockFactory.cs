using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests;

public static class MockFactory
{
    internal static IMutableFileSystemFolder CreateFolder(
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
        folder.Contents.Returns(contentsObservable);
        folder.GetOrLoadContents().Returns(contentsObservable);
        folder.GetOrLoadContentsAsync().Returns(Task.FromResult(contentsObservable));

        return folder;
    }

    internal static (IFileSystemFile file, IMutableFileSystemItem mutable) CreateFile(FileSystemPath? path = null, IMutableFileSystemFolder? parent = null)
    {
        var file = Substitute.For<IMutableFileSystemItem, IFileSystemFile>();

        path ??= CreatePath();

        file.Path.Returns(path.Value);
        file.ParentFolder.Returns(parent);

        return ((IFileSystemFile)file, (IMutableFileSystemItem)file);
    }

    internal static IMutableFileSystemItem CreateItem(FileSystemPath? path = null, IMutableFileSystemFolder? parent = null)
    {
        var item = Substitute.For<IMutableFileSystemItem>();

        path ??= CreatePath();

        item.Path.Returns(path.Value);
        item.ParentFolder.Returns(parent);

        return item;
    }

    internal static FileSystemPath CreatePath(FileSystemPath? parent = null, string? name = null)
    {
        parent ??= "GlobalMockParent";
        name ??= Random.Shared.Next().ToString();
        var path = parent.Value.ChildPath(name);
        
        return path;
    }

    internal static IPersistentDictionary CreateItemData(string name = "Item", bool isFolder = false)
    {
        var dictionary = Substitute.For<IPersistentDictionary>();

        var namePoint = CreateDataPoint(name);
        var folderPoint = CreateDataPoint(isFolder);

        dictionary[IFileSystemItemFactory.PersistenceNameKey].Returns(namePoint);
        dictionary[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(folderPoint);

        return dictionary;
    }

    internal static IPersistentDataPoint CreateDataPoint<T>(T value) where T : notnull
    {
        var point = Substitute.For<IPersistentDataPoint>();
        var persistentValue = Substitute.For<IPersistentValue<T>>();

        persistentValue.Value.Returns(value);
        point.GetValue<T>().Returns(persistentValue);

        return point;
    }
}