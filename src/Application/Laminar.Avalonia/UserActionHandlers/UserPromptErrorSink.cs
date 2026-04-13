using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Exceptions;
using System.Threading.Tasks;

namespace Laminar.Avalonia.UserActionHandlers;

internal class UserPromptErrorSink(DialogService dialogService) : IUnresolvedUserActionErrorSink
{
    public Task OnError(IUserAction action, UserActionError error) 
    {
        (string errorTitle, string errorMessage) = error.Exception switch
        {
            InvalidStorageItemNameException storageItemName => ("Item rename error", $"Invalid storage item name: '{storageItemName.Name}'"),
            _ => ("Error execting action", error.Exception.Message)
        };

        return dialogService.PromptError(errorTitle, errorMessage); 
    }
}
