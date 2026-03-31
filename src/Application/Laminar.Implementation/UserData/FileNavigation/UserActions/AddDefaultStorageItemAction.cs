using System;
using System.IO;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class AddDefaultStorageItemAction<T>(IFileSystem fileSystem, ILaminarStorageFolder parentFolder, ILaminarStorageItemFactory factory) : IUserAction
    where T : class, ILaminarStorageItem
{
    private static readonly (Type, string)[] DefaultItemNames =
    [
        (typeof(ILaminarStorageFolder), "Untitled Folder"),
        (typeof(ILaminarStorageItem), "Untitled Script.pls"),
    ];
    
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute => true;
    public IUserActionResult Execute()
    {
        string newItemPath = Path.Join(parentFolder.Path, GetDefaultItemName());
        ILaminarStorageItem newItem = factory.FromPath(newItemPath, parentFolder);
        newItem.NeedsName = true;
        newItem.ParentFolder?.Refresh();
        return IUserActionResult.Success(new DeleteStorageItemAction<T>(fileSystem, newItem));
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