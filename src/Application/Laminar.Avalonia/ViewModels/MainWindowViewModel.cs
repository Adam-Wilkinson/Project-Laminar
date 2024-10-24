using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Laminar.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private bool _settingsOpen;
    private readonly SettingsOverlay _settingsOverlay = new();
    private readonly Control _laminarEditor = new TextBlock { Text = "This will be Project: Laminar", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

    public MainWindowViewModel()
    {
        WindowCentralControl = _laminarEditor;
    }

    public Control WindowCentralControl { get; set; }

    public bool SettingsOpen
    {
        get
        {
            return _settingsOpen;
        }
        set
        {
            if (_settingsOpen != value)
            {
                WindowCentralControl = _settingsOpen ? _laminarEditor : _settingsOverlay;
                OnPropertyChanged(nameof(WindowCentralControl));
                OnPropertyChanged();
                _settingsOpen = value;
            }
        }
    }
}
