using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemGraph(
    IWritableFileSystemItemRepository repository,
    IFileSystemItemFactory itemFactory) : IFileSystemGraph
{
    private static MutationToken _token = null!;

    public void Move(IFileSystemItem item, IFileSystemFolder newParent, int newIndex)
    {
        if (Equals(item.ParentFolder, newParent))
        {
            ToMutable(newParent).MoveChildInternal(_token, item.ParentFolder!.LoadOrGetContents().IndexOf(item), newIndex);
            return;
        }
        
        repository.Remove(_token, item);
        
        ToMutable(item.ParentFolder).RemoveChildInternal(_token, item);
        ToMutable(newParent).InsertChildInternal(_token, item, newIndex);
        ToMutable(item).SetParentInternal(_token, newParent);
        
        repository.Add(_token, item);
    }

    public void Rename(IFileSystemItem item, string newNameAndExtension)
    {
        repository.Remove(_token, item);
        ToMutable(item).SetNameInternal(_token, newNameAndExtension);
        repository.Add(_token, item);
    }

    public void Delete(IFileSystemItem item)
    {
        repository.Remove(_token, item);
        ToMutable(item.ParentFolder).RemoveChildInternal(_token, item);
        ToMutable(item).RaiseOnDeleted();
    }

    public IFileSystemFolder AddFolder(IFileSystemFolder parent, int indexInParent, string name)
    {
        var newFolder = itemFactory.CreateFolder(parent, name);
        AddItemInternal(parent, indexInParent, newFolder);
        return newFolder;
    }

    public IFileSystemFile AddFile(IFileSystemFolder parent, int indexInParent, string nameAndExtension)
    {
        var newFile = itemFactory.CreateFile(parent, nameAndExtension);
        AddItemInternal(parent, indexInParent, newFile);
        return newFile;
    }

    public IFileSystemItem AddFromPersistentData(IFileSystemFolder parent, IPersistentDictionary persistentDictionary)
    {
        var childName = persistentDictionary[IFileSystemItemFactory.PersistenceNameKey].GetValue<string>().Value;
        if (repository.TryGetExisting(parent.Path.ChildPath(childName), out _))
        {
            throw new InvalidOperationException("An item already exists at this path. This is likely an internal bug");
        }

        var newItem = itemFactory.CreateFromPersistentData(parent, persistentDictionary);
        AddItemInternal(parent, parent.LoadOrGetContents().Count, newItem);
        return newItem;
    }

    private void AddItemInternal(IFileSystemFolder parent, int indexInParent, IFileSystemItem item)
    {
        ToMutable(parent).InsertChildInternal(_token, item, indexInParent);
        repository.Add(_token, item);
    }

    private static IMutableFileSystemItem ToMutable(IFileSystemItem? item)
    {
        if (item is not IMutableFileSystemItem mutable)
        {
            throw new InvalidOperationException("The storage graph can only change mutable storage items");
        }

        return mutable;
    }
    
    private static IMutableFileSystemFolder ToMutable(IFileSystemFolder? item)
    {
        if (item is not IMutableFileSystemFolder mutable)
        {
            throw new InvalidOperationException("The storage graph can only change mutable storage items");
        }

        return mutable;
    }
    
    internal class MutationToken
    {
        static MutationToken()
        {
            _token = new MutationToken();
        }

        private MutationToken()
        {
        }
    }
}