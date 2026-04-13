using Laminar.Contracts.Base.ActionSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

internal class DeleteStorageItemAction : IUserAction
{
    public bool CanExecute => false;

    public event EventHandler? CanExecuteChanged;

    public Task<IUserActionResult> Execute()
    {
        return Task.FromResult(IUserActionResult.Cancelled());
    }
}
