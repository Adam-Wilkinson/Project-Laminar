using CommunityToolkit.Mvvm.Input;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IClipboardProvider
{
    public IAsyncRelayCommand CopyToClipboardCommand { get; }
}