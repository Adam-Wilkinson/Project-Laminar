using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels.Services;

public class DialogService(TopLevel topLevel, IDialogService dialogService)
{
    private INotifyPropertyChanged? TopLevelViewModel => field ??= (INotifyPropertyChanged)topLevel?.DataContext;

    public async Task<DialogOption> PromptUserResponse(LaminarDialogViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(TopLevelViewModel);
        await dialogService.ShowDialogAsync(TopLevelViewModel, viewModel);
        return viewModel.SelectedOption;
    }

    public async Task<bool> PromptYesNo(string title, string message) 
        => (await PromptUserResponse(new LaminarDialogViewModel
    {
        Options = [ DialogOption.No, DialogOption.Yes ],
        Title = title,
        Message = message,
        SelectedOptionIndex = 1,
        CancelledOptionIndex = 0,
    })) == DialogOption.Yes;

    public async Task<bool> PromptCancelOk(string title, string message) => 
        (await PromptUserResponse(new LaminarDialogViewModel
    {
        Options = [ DialogOption.Cancel, DialogOption.Ok ],
        Title = title,
        Message = message,
        SelectedOptionIndex = 1,
        CancelledOptionIndex = 0,
    })) == DialogOption.Ok;

    public async Task PromptError(string title, string message) => await PromptUserResponse(new LaminarDialogViewModel
    {
        Options = [ DialogOption.Ok ],
        Title = title,
        Message = message,
    });

    public async Task<FileSystemPath?> PromptForFolder(WellKnownFolder? startingLocation = null)
    {
        var selected = await dialogService.ShowOpenFolderDialogAsync(TopLevelViewModel, new OpenFolderDialogSettings
        {
            SuggestedStartLocation = await GetWellKnownFolder(startingLocation)
        });

        if (selected is null) return null;
        return new FileSystemPath(selected.LocalPath);
    }
    
    private async Task<IDialogStorageFolder?> GetWellKnownFolder(WellKnownFolder? startingLocation)
    {
        if (startingLocation is not { } knownStart) return null;
        
        var result = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(knownStart);
        return result?.ToDialog();
    }
}
