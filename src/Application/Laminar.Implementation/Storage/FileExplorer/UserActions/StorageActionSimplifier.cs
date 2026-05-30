using System.Data;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

public class StorageActionSimplifier(FileExplorerActionDependencies dependencies) : IUserActionSimplifier
{
    public IUserActionSimplification Simplify(IUserAction first, IUserAction second) => (first, second) switch
    {
        (AddRootFolderAction addRootFolderFirst, RemoveRootFolderAction removeRootFolderSecond) 
            when addRootFolderFirst.RootFolderPath == removeRootFolderSecond.RootFolderPath => IUserActionSimplification.Undoes(),
        
        (RemoveRootFolderAction removeRootFolderFirst, AddRootFolderAction addRootFolderSecond)
            when addRootFolderSecond.RootFolderPath == removeRootFolderFirst.RootFolderPath => IUserActionSimplification.Undoes(),
        
        (DeleteStorageItemAction deleteFirst, AddStorageItemAction addSecond) => DeleteThenAdd(deleteFirst, addSecond),
        
        (AddStorageItemAction addFirst, DeleteStorageItemAction deleteSecond)
            when deleteSecond.Target.Path.NameAndExtension == addFirst.ItemNameAndExtension && deleteSecond.Target.ParentFolder == addFirst.Parent
            => IUserActionSimplification.Undoes(),
        
        (MoveStorageItemAction moveFirst, MoveStorageItemAction moveSecond)
            when moveFirst.Target ==  moveSecond.Target => IUserActionSimplification.Overrides(),
        
        (RenameStorageItemAction renameFirst, RenameStorageItemAction renameSecond)
            when renameFirst.Target == renameSecond.Target => IUserActionSimplification.Overrides(),
        
        _ => IUserActionSimplification.None(),
    };

    private IUserActionSimplification DeleteThenAdd(DeleteStorageItemAction delete, AddStorageItemAction add)
    {
        if (delete.Target.Path.NameAndExtension != add.ItemNameAndExtension ||
            delete.Target.ParentFolder != add.Parent) 
            return IUserActionSimplification.None();
        
        if (add.Parent.Contents.IndexOf(delete.Target) == add.IndexInParent)
        {
            return IUserActionSimplification.Undoes();
        }

        return IUserActionSimplification.NewEffectiveAction(new MoveStorageItemAction(delete.Target, add.Parent, add.IndexInParent, dependencies));
    }
}