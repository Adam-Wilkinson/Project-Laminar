using System;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class AddDefaultStorageItemAction<T>(
    ILaminarStorageFolder parentFolder, 
    FileExplorerActionDependencies dependencies) 
    : IUserAction where T : class, ILaminarStorageItem
{
    private static readonly (Type, string)[] DefaultItemNames =
    [
        (typeof(ILaminarStorageFolder), "Untitled Folder"),
        (typeof(ILaminarStorageItem), "Untitled Script.pls"),
    ];

    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        FileSystemPath newItemPath = parentFolder.Path.ChildPath(GetDefaultItemName());
        if (dependencies.StorageItemFactory.FromPath(newItemPath, parentFolder) is not LaminarStorageItem newItem) 
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidOperationException()));
        }

        newItem.NeedsName = true;
        return Task.FromResult(IUserActionResult.Success(new DeleteStorageItemAction(newItem, dependencies)));
    }
    
    private static string GetDefaultItemName()
    {
        foreach (var (type, name) in DefaultItemNames)
        {
            if (type.IsAssignableFrom(typeof(T)))
            {
                return name;
            }
        }

        return "";
    }
}