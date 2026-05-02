using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;

namespace Laminar.Avalonia.ViewModels;

public partial class NodePickerViewModel : ViewModelBase
{
    public string Name => "A nose picker";

    [Persistent, ObservableProperty] 
    public partial double Height { get; set; } = 200;
}