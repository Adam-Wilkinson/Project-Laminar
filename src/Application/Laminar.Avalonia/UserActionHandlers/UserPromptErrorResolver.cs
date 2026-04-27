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
        ResolvableError<DeleteRootFolderConfirmation>
        {
            Exception: DeleteRootFolderException deleteRootFolderException
        } deleteRootFolderConfirmation => ResolveError(deleteRootFolderConfirmation, new LaminarDialogViewModel
        {
            Options = [
                DialogOption.Cancel, 
                new DialogOption<DeleteRootFolderConfirmation>(DeleteRootFolderConfirmation.RemoveRootFolder, "Remove from root folders"),
                new DialogOption<DeleteRootFolderConfirmation>(DeleteRootFolderConfirmation.DeleteRootFolder, "Delete permanently")],
            Message = $"You are attempting to delete the folder '{deleteRootFolderException.FolderPath.Name}'. This will permanently delete this folder and all its children.\r\n\r\nYou can also remove this folder from Project: Laminar without deleting it from your file system",
            SelectedOptionIndex = 1,
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
            resolvable.OnCancelled?.Invoke();
            return new UserActionCancelledResolution();
        }

        throw new InvalidOperationException();
    }
}
