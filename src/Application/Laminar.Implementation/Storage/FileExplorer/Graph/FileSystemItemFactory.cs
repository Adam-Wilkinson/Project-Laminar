using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal sealed class FileSystemItemFactory(IServiceProvider provider, IEncodableDataFactory dataFactory)
    : IFileSystemItemFactory
{
    public IFileSystemFile CreateFile(IFileSystemFolder parent, string nameAndExtension) 
        => CreateFileInternal(parent, CreatePersistentData(nameAndExtension, false));

    public IFileSystemFolder CreateFolder(IFileSystemFolder parent, string name) 
        => CreateFolderInternal(parent, CreatePersistentData(name, true));
    
    public IFileSystemRootFolder CreateRootFolder(FileSystemPath path, IPersistentDictionary data) 
        => ActivatorUtilities.CreateInstance<FileSystemRootFolder>(provider, path, CreatePersistentData(path.NameAndExtension, true, data));
    
    public IFileSystemItem CreateFromPersistentData(IFileSystemFolder parent, IPersistentDictionary persistentDictionary)
    {
        if (!persistentDictionary[IFileSystemItemFactory.PersistenceIsFolderKey].GetValue<bool>()
                .TryGetResult(out var result, out var error))
        {
            throw new InvalidOperationException("This storage item has not stored if it is a folder", error);
        }

        return result.Value
            ? CreateFolderInternal(parent, persistentDictionary)
            : CreateFileInternal(parent, persistentDictionary);
    }

    private IPersistentDictionary CreatePersistentData(string nameAndExtension, bool isFolder, IPersistentDictionary? existingData = null)
    {
        existingData ??= dataFactory.GetEncodableData<IPersistentDictionary>();
        existingData[IFileSystemItemFactory.PersistenceNameKey].GetValueOrInitialize(nameAndExtension);
        existingData[IFileSystemItemFactory.PersistenceIsFolderKey].GetValueOrInitialize(isFolder);
        return existingData;
    }
    
    private FileSystemFolder CreateFolderInternal(IFileSystemFolder parent, IPersistentDictionary persistentDictionary)
        => ActivatorUtilities.CreateInstance<FileSystemFolder>(provider, parent, persistentDictionary);
    
    private FileSystemFile CreateFileInternal(IFileSystemFolder parent, IPersistentDictionary persistentDictionary)
        =>  ActivatorUtilities.CreateInstance<FileSystemFile>(provider, parent, persistentDictionary);
}