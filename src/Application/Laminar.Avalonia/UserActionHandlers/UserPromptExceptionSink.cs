using System;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Domain.Exceptions;
using System.Threading.Tasks;
using Laminar.Contracts.Base;

namespace Laminar.Avalonia.UserActionHandlers;

internal class UserPromptExceptionSink(DialogService dialogService) : IExceptionSink
{
    public Task OnException(Exception exception) 
    {
        (string errorTitle, string errorMessage) = exception switch
        {
            InvalidStorageItemNameException {Name: var name} => ("Item rename error", $"Invalid storage item name: '{name}'"),
            ErrorDecodingValueException => ("Error decoding value", $"A value could not be properly decoded from file. Overriding with old value"),
            CannotMoveRootFolderException {FolderName: var folderName} => ("Cannot move item", $"The storage item '{folderName}' is a root folder, and therefore cannot be moved"),
            _ => ("Error executing action", exception.Message)
        };

        return dialogService.PromptError(errorTitle, errorMessage); 
    }
}
