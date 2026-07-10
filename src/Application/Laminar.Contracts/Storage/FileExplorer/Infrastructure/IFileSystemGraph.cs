using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemGraph
{
    public void Move(IFileSystemItem item, IFileSystemFolder newParent, int newIndex);

    public void Rename(IFileSystemItem item, string newNameAndExtension);

    public void Delete(IFileSystemItem item);

    public IFileSystemFolder AddFolder(IFileSystemFolder parent, int indexInParent, string name);
    
    public IFileSystemFile AddFile(IFileSystemFolder parent, int indexInParent, string nameAndExtension);

    public IFileSystemItem AddFromPersistentData(IFileSystemFolder parent, IPersistentDictionary persistentData);
}