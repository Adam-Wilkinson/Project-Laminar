using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Design;

public class DesignFileBrowser : ILaminarFileBrowser
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; } 
        = new ObservableCollection<ILaminarStorageRootFolder>().ToInterfaceImpl();

    public async Task<IUserActionResult> AddDefault<T>(ILaminarStorageFolder parentFolder)
        where T : class, ILaminarStorageItem
        => IUserActionResult.Invalid();

    public async Task<IUserActionResult> Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex) 
        => IUserActionResult.Invalid();

    public async Task<IUserActionResult> Delete(ILaminarStorageItem itemToDelete) 
        => IUserActionResult.Invalid();

    public async Task<IUserActionResult> Rename(ILaminarStorageItem itemToRename, string newName) 
        => IUserActionResult.Invalid();

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item) => false;

    public async Task<IUserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath)
        => IUserActionResult.Invalid();

    public async Task<IUserActionResult> AddRootFolder(FileSystemPath newRootFolderPath)
        => IUserActionResult.Invalid();
}