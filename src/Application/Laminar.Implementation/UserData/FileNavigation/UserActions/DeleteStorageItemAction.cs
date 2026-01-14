using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class DeleteStorageItemAction<T>(T item) : IUserAction where T : class, ILaminarStorageItem
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute => item.ParentFolder is not null;
    public IUserAction? Execute()
    {
        if (item.ParentFolder is not { } parentFolder) return null;
        item.Delete();
        return new AddStorageItemAction<T>(item, parentFolder);
    }
}