using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace Laminar.Avalonia.UserActionHandlers;

public class UserPromptErrorResolver(DialogService dialogService) : IUserActionErrorResolver
{
    private readonly DialogOption<NamingConflictResolution> ReplaceExistingFile = new(NamingConflictResolution.ReplaceItem, "Replace file");
    private readonly DialogOption<NamingConflictResolution> IncrementFileName = new(NamingConflictResolution.IncrementName, "Increment name");

    public Task<IUserActionErrorResolution?> TryResolve(IUserActionResult result) => result switch
    {
        ResolvableError<NamingConflictResolution> 
        { 
            Exception: FileWithNameExistsException fileWithNameExists 
        } renamingNameClash => ResolveError(renamingNameClash, new LaminarDialogViewModel
        {
            Options = [DialogOption.Cancel, ReplaceExistingFile, IncrementFileName],
            Title = "Rename error",
            Message = $"A file with the name '{fileWithNameExists.Name}' already exists in that folder",
            SelectedOptionIndex = 2,
            CancelledOptionIndex = 0,
        }),
        ResolvableError<NamingConflictResolution> 
        { 
            Exception: DestinationContainsItemOfThatNameException destinationContainsException 
        } moveNamingConflict => ResolveError(moveNamingConflict, new LaminarDialogViewModel
        {
            Options = [DialogOption.Cancel, ReplaceExistingFile, IncrementFileName],
            Title = "Error moving item",
            Message = $"Destination folder '{destinationContainsException.DestinationFolder}' already contains an item of name '{destinationContainsException.ItemName}'",
            SelectedOptionIndex = 2,
            CancelledOptionIndex = 0,
        }),
        _ => Task.FromResult<IUserActionErrorResolution?>(null),
    };

    private async Task<IUserActionErrorResolution?> ResolveError<T>(ResolvableError<T> resolvable, LaminarDialogViewModel dialog)
    {
        var parameterDialogOption = await dialogService.PromptUserResponse(dialog);
        if (parameterDialogOption is DialogOption<T> typedCorrectly)
        {
            return resolvable.Resolve(typedCorrectly.Value);
        }

        if (parameterDialogOption == DialogOption.Cancel)
        {
            return new UserActionCancelledResolution();
        }

        throw new InvalidOperationException();
    }
}
