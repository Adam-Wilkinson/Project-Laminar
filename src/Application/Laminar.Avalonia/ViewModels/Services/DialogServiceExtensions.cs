using System.ComponentModel;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;

namespace Laminar.Avalonia.ViewModels.Services;

public static class DialogServiceExtensions
{
    extension(IDialogService dialogService)
    {
        public async Task ShowError(INotifyPropertyChanged mainWindowViewModel, string title, string message)
        {
            await dialogService.ShowMessageBoxAsync(mainWindowViewModel, message, title, icon: MessageBoxImage.Error);
        }
    }
}