using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.Shapes;
using Laminar.Avalonia.ToolSystem;
using Laminar.Domain.ValueObjects;
using Point = Avalonia.Point;

namespace Laminar.Avalonia.ViewModels;

public class TitleBarViewModel : ViewModelBase
{
    public MainWindowViewModel? MainWindow { get; set; }
}