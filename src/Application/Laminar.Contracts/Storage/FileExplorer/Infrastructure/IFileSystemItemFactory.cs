using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemItemFactory
{
    public const string PersistenceNameKey = "Name";
    public const string PersistenceIsFolderKey = "IsFolder";
    
    public IFileSystemFile CreateFile(IFileSystemFolder parent, string nameAndExtension);
    
    public IFileSystemFolder CreateFolder(IFileSystemFolder parent, string name);
    
    public IFileSystemRootFolder CreateRootFolder(FileSystemPath path);
    
    public IFileSystemItem CreateFromPersistentData(IFileSystemFolder parent, IPersistentDictionary persistentDictionary);
}