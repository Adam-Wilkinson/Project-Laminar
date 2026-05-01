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
    private static readonly ValueDialogOption<NamingConflictResolution> ReplaceExistingFile = new(NamingConflictResolution.ReplaceItem, "Replace file");
    private static readonly ValueDialogOption<NamingConflictResolution> IncrementFileName = new(NamingConflictResolution.IncrementName, "Increment name");

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
            Exception: DeleteRootFolderException { FolderPath: var deletedFolderPath }
        } deleteRootFolderConfirmation => ResolveError(deleteRootFolderConfirmation, new LaminarDialogViewModel
        {
            Options = [
                DialogOption.Cancel, 
                DialogOption.WithValue(DeleteRootFolderConfirmation.RemoveRootFolder, "Forget root folder"),
                // DialogOption.WithValue(DeleteRootFolderConfirmation.RemoveRootFolderAndCleanup, "Forget root folder and related information"),
                DialogOption.WithValue(DeleteRootFolderConfirmation.DeleteRootFolder, "Delete permanently")],
            Message = $"You are attempting to delete the root folder '{deletedFolderPath.NameAndExtension}'. This will permanently delete this folder and all its children.\r\n\r\nYou can also remove this folder from Project: Laminar without deleting it from your file system",
            AdditionalCheckboxText = "Also delete Project: Laminar information files",
            AdditionalCheckboxChecked = true,
            SelectedOptionIndex = 1,
            CancelledOptionIndex = 0,
        }, (answer, isChecked) => (answer, isChecked) switch
        {
            (DeleteRootFolderConfirmation.DeleteRootFolder, _) => DeleteRootFolderConfirmation.DeleteRootFolder,
            (DeleteRootFolderConfirmation.RemoveRootFolder, true) => DeleteRootFolderConfirmation.RemoveRootFolderAndCleanup,
            (DeleteRootFolderConfirmation.RemoveRootFolder, false) => DeleteRootFolderConfirmation.RemoveRootFolder,
            _ => throw new NotImplementedException()
        }),
        _ => Task.FromResult<IUserActionErrorResolution?>(null),
    };

    private async Task<IUserActionErrorResolution?> ResolveError<T>(
        ResolvableError<T> resolvable, 
        LaminarDialogViewModel dialog,
        Func<T, bool, T>? isCheckedFilter = null)
    {
        var parameterDialogOption = await dialogService.PromptUserResponse(dialog);
        if (parameterDialogOption is ValueDialogOption<T> typedCorrectly)
        {
            var result = isCheckedFilter is null
                ? typedCorrectly.Value
                : isCheckedFilter(typedCorrectly.Value, dialog.AdditionalCheckboxChecked);
            
            return resolvable.Resolve(result);
        }

        if (parameterDialogOption == DialogOption.Cancel)
        {
            resolvable.OnCancelled?.Invoke();
            return new UserActionCancelledResolution();
        }

        throw new InvalidOperationException();
    }
}
