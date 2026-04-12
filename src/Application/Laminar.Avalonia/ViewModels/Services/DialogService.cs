using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

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
}
