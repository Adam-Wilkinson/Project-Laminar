using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Graph;

public interface IFileSystemRoots : IReadOnlyObservableList<IFileSystemRootFolder>
{
    public bool TryGetRootFolder(FileSystemPath path, [MaybeNullWhen(false)] out IFileSystemRootFolder folder);
    
    public IFileSystemRootFolder CreateRootFolder(FileSystemPath path, IPersistentDictionary data);

    public bool RemoveRootAt(FileSystemPath path, bool deleteInfoFiles);

    public bool RemoveRootAt(FileSystemPath path, bool deleteInfoFiles, out IFileSystemRootFolder? oldRoot);
}