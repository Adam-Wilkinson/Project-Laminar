using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Design;

public class DesignFileBrowser : IFileBrowser
{
    public IReadOnlyObservableCollection<IFileSystemRootFolder> RootFolders { get; } 
        = new ObservableCollection<IFileSystemRootFolder>().ToInterfaceImpl();

    public async Task<IUserActionResult> Add(string itemName, IFileSystemFolder parentFolder, int indexInParent, FileSystemItemType type)
        => IUserActionResult.Ineffectual();

    public async Task<IUserActionResult> Move(IFileSystemItem itemToMove, IFileSystemFolder destinationFolder, int destinationIndex) 
        => IUserActionResult.Ineffectual();

    public async Task<IUserActionResult> Delete(IFileSystemItem itemToDelete) 
        => IUserActionResult.Ineffectual();

    public async Task<IUserActionResult> Rename(IFileSystemItem itemToRename, string newName) 
        => IUserActionResult.Ineffectual();

    public bool OpenInSystemFileBrowser(IFileSystemItem item) => false;

    public async Task<IUserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath)
        => IUserActionResult.Ineffectual();

    public async Task<IUserActionResult> AddRootFolder(FileSystemPath newRootFolderPath)
        => IUserActionResult.Ineffectual();
}