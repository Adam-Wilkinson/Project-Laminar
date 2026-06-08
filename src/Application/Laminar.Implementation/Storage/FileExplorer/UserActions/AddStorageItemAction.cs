using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct AddStorageItemAction(
    string newItemName, 
    LaminarStorageFolder parent, 
    int indexInParent,
    StorageItemType itemType,
    FileExplorerActionDependencies dependencies) 
    : IUserAction
{
    public string ItemNameAndExtension { get; } = newItemName + GetExtension(itemType);

    public LaminarStorageFolder Parent { get; } = parent;
    
    public int IndexInParent => indexInParent;
    
    public bool CanExecute => true;
        
    public Task<IUserActionResult> Execute()
    {
        var result = dependencies.StorageItemFactory.CreateChild(ItemNameAndExtension, Parent,
            itemType is StorageItemType.Folder);
        
        (Parent.Contents as IObservableCollection<ILaminarStorageItem>)?.Insert(indexInParent, result);
        
        if (result is not LaminarStorageItem storageItemInternal) throw new InvalidOperationException();
        return Task.FromResult(IUserActionResult.Success(result, new DeleteStorageItemAction(storageItemInternal, dependencies)));
    }

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
    {
        if (previousAction is not DeleteStorageItemAction removeAction ||
            removeAction.Target.Path.NameAndExtension != ItemNameAndExtension ||
            removeAction.Target.ParentFolder != Parent) 
            return IUserActionSimplification.None();
        
        if (Parent.Contents.IndexOf(removeAction.Target) == indexInParent)
        {
            return IUserActionSimplification.Undoes();
        }

        return IUserActionSimplification.NewEffectiveAction(new MoveStorageItemAction(removeAction.Target, Parent, indexInParent, dependencies));
    }

    private static string GetExtension(StorageItemType type) => type switch
    {
        StorageItemType.Folder => string.Empty,
        StorageItemType.Script => ".pls",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}