using CommunityToolkit.Mvvm.Input;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface IClipboardProvider
{
    public IAsyncRelayCommand CopyToClipboardCommand { get; }

    public IAsyncRelayCommand PasteFromClipboardCommand { get; }

    public IAsyncRelayCommand CutCommand { get; }
}