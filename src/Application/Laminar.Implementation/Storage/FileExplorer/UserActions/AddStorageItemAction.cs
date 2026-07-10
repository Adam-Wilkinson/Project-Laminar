using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Notification.Collections;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct AddStorageItemAction(
    string newItemName, 
    IFileSystemFolder parent, 
    int indexInParent,
    FileSystemItemType itemType,
    FileBrowserActionDependencies dependencies) 
    : IUserAction
{
    public string ItemNameAndExtension { get; } = newItemName + itemType.Extension;

    public IFileSystemFolder Parent { get; } = parent;
    
    public int IndexInParent => indexInParent;
    
    public bool CanExecute => true;
        
    public Task<IUserActionResult> Execute()
    {
        IFileSystemItem newItem = itemType.IsFolder
            ? dependencies.CommandService.AddFolder(Parent, indexInParent, ItemNameAndExtension)
            : dependencies.CommandService.AddFile(Parent, indexInParent, ItemNameAndExtension);
        
        return Task.FromResult(IUserActionResult.Success(newItem, new DeleteStorageItemAction(newItem, dependencies)));
    }
}