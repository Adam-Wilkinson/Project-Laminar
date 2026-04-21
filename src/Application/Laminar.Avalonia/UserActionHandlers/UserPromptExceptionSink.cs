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
            InvalidStorageItemNameException storageItemName => ("Item rename error", $"Invalid storage item name: '{storageItemName.Name}'"),
            ErrorDecodingValueException decodingValue => ("Error decoding value", $"A value could not be properly decoded from file. Overriding with old value"),
            _ => ("Error executing action", exception.Message)
        };

        return dialogService.PromptError(errorTitle, errorMessage); 
    }
}
