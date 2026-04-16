using System;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

internal class AddDefaultStorageItemAction<T>(
    ILaminarStorageFolder parentFolder, 
    ILaminarStorageItemFactory factory,
    ILaminarStorageRootFolder recyclingBin) 
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
        if (factory.FromPath(newItemPath, parentFolder) is not LaminarStorageItem newItem) 
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidOperationException()));
        }

        newItem.NeedsName = true;
        return Task.FromResult(IUserActionResult.Success(new DeleteStorageItemAction(newItem, recyclingBin)));
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