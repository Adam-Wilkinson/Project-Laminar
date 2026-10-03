using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Observables;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Design;

public class DesignFileBrowser : IFileBrowser
{
    public IReadOnlyObservableList<IFileSystemRootFolder> RootFolders { get; } 
        = new ObservableList<IFileSystemRootFolder>();

    public IUserActionScope ActionScope => throw new InvalidOperationException();

    public async Task<UserActionResult> Add(string itemName, IFileSystemFolder parentFolder, int indexInParent, FileSystemItemType type)
        => UserActionResult.Success();

    public async Task<UserActionResult> Move(IFileSystemItem itemToMove, IFileSystemFolder destinationFolder, int destinationIndex) 
        => UserActionResult.Success();

    public async Task<UserActionResult> Delete(IFileSystemItem itemToDelete) 
        => UserActionResult.Success();

    public async Task<UserActionResult> Rename(IFileSystemItem itemToRename, string newName) 
        => UserActionResult.Success();

    public bool OpenInSystemFileBrowser(IFileSystemItem item) => false;

    public async Task<UserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath)
        => UserActionResult.Success();

    public async Task<UserActionResult> AddRootFolder(FileSystemPath newRootFolderPath)
        => UserActionResult.Success();
}