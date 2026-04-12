using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.UserData.FileNavigation.Exceptions;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using System.Threading.Tasks;

namespace Laminar.Avalonia.UserActionHandlers;

public class UserPromptErrorResolver(DialogService dialogService) : IUserActionErrorResolver
{
    private readonly DialogOption ReplaceExistingFile = new("Replace file");
    private readonly DialogOption IncrementFileName = new("Increment name");

    public async Task<IUserActionErrorResolution?> TryResolve(IUserAction action, UserActionError error)
    {
        switch (error.Exception)
        {
            case InvalidStorageItemNameException storageItemName:
                await dialogService.PromptError("Invalid item name", $"Invalid storage item name: '{storageItemName.Name}'");
                return new UserActionCancelledResolution();
            case FileWithNameExistsException fileWithNameExists:
                await dialogService.PromptUserResponse(new LaminarDialogViewModel
                {
                    Options = [DialogOption.Cancel, ReplaceExistingFile, IncrementFileName],
                    Title = "Rename error",
                    Message = $"A file with the name '{fileWithNameExists.Name}' already exists in that folder",
                    SelectedOptionIndex = 2,
                    CancelledOptionIndex = 0,
                });

                return new UserActionCancelledResolution();
            case DestinationContainsItemOfThatNameException destinationContainsException:
                await dialogService.PromptUserResponse(new LaminarDialogViewModel
                {
                    Options = [DialogOption.Cancel, ReplaceExistingFile, IncrementFileName],
                    Title = "Rename error",
                    Message = $"Destination folder '{destinationContainsException.DestinationFolder}' already contains an item of name '{destinationContainsException.ItemName}'",
                    SelectedOptionIndex = 2,
                    CancelledOptionIndex = 0,
                });

                return new UserActionCancelledResolution();
        }

        return null;
    }
}
