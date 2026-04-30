using System;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class AddStorageItemAction(
    string newItemName, 
    LaminarStorageFolder parent, 
    StorageItemType itemType, 
    FileExplorerActionDependencies dependencies) 
    : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    
    public bool CanExecute => true;
        
    public Task<IUserActionResult> Execute()
    {
        var result = dependencies.StorageItemFactory.CreateChild(newItemName + GetExtension(itemType), parent,
            itemType is StorageItemType.Folder);
        if (result is not LaminarStorageItem storageItemInternal) throw new InvalidOperationException();
        return Task.FromResult(IUserActionResult.Success(result, new DeleteStorageItemAction(storageItemInternal, dependencies)));
    }
    
    private static string GetExtension(StorageItemType type) => type switch
    {
        StorageItemType.Folder => string.Empty,
        StorageItemType.Script => ".pls",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}