using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Notification;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct AddStorageItemAction(
    string newItemName, 
    LaminarStorageFolder parent, 
    int indexInParent,
    StorageItemType itemType,
    FileExplorerActionDependencies dependencies) 
    : IUserAction
{
    public string ItemName { get; } = newItemName;

    public LaminarStorageFolder Parent { get; } = parent;
    
    public bool CanExecute => true;
        
    public Task<IUserActionResult> Execute()
    {
        var result = dependencies.StorageItemFactory.CreateChild(ItemName + GetExtension(itemType), Parent,
            itemType is StorageItemType.Folder);
        
        (Parent.Contents as IObservableCollection<ILaminarStorageItem>)?.Insert(indexInParent, result);
        
        if (result is not LaminarStorageItem storageItemInternal) throw new InvalidOperationException();
        return Task.FromResult(IUserActionResult.Success(result, new DeleteStorageItemAction(storageItemInternal, dependencies)));
    }

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
        => IUserActionSimplification.None();

    private static string GetExtension(StorageItemType type) => type switch
    {
        StorageItemType.Folder => string.Empty,
        StorageItemType.Script => ".pls",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}