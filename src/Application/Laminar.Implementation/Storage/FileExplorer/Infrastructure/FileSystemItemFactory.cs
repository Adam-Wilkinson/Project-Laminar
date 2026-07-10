using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemItemFactory(IServiceProvider provider, IEncodableDataFactory dataFactory)
    : IFileSystemItemFactory
{
    public IFileSystemFile CreateFile(IFileSystemFolder parent, string nameAndExtension) 
        => CreateFileInternal(parent, CreatePersistentData(nameAndExtension, false));

    public IFileSystemFolder CreateFolder(IFileSystemFolder parent, string name) 
        => CreateFolderInternal(parent, CreatePersistentData(name, true));
    
    public IFileSystemItem CreateFromPersistentData(IFileSystemFolder parent, IPersistentDictionary persistentDictionary) 
        => persistentDictionary[IFileSystemItemFactory.PersistenceIsFolderKey].GetValue<bool>().Value
            ? CreateFolderInternal(parent, persistentDictionary)
            : CreateFileInternal(parent, persistentDictionary);
    
    public IFileSystemRootFolder CreateRootFolder(FileSystemPath path) 
        => ActivatorUtilities.CreateInstance<FileSystemRootFolder>(provider, path, CreatePersistentData(path.NameAndExtension, true));

    private IPersistentDictionary CreatePersistentData(string nameAndExtension, bool isFolder)
    {
        var newPersistentData = dataFactory.GetEncodableData<IPersistentDictionary>();
        newPersistentData[IFileSystemItemFactory.PersistenceNameKey].GetValueOrInitialize(nameAndExtension);
        newPersistentData[IFileSystemItemFactory.PersistenceIsFolderKey].GetValueOrInitialize(isFolder);
        return newPersistentData;
    }
    
    private FileSystemFolder CreateFolderInternal(IFileSystemFolder parent, IPersistentDictionary persistentDictionary)
        => ActivatorUtilities.CreateInstance<FileSystemFolder>(provider, parent, persistentDictionary);
    
    private FileSystemFile CreateFileInternal(IFileSystemFolder parent, IPersistentDictionary persistentDictionary)
        =>  ActivatorUtilities.CreateInstance<FileSystemFile>(provider, parent, persistentDictionary);
}