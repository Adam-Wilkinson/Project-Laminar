using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using System.Threading.Tasks;

namespace Laminar.Avalonia.UserActionHandlers;

internal class UserPromptErrorSink(DialogService dialogService) : IUnresolvedUserActionErrorSink
{
    public Task OnError(IUserAction action, UserActionError error) 
        => dialogService.PromptError("Error while executing action", error.Exception.Message);
}
