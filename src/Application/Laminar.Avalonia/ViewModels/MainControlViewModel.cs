using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Avalonia.ViewModels;
public partial class MainControlViewModel : ViewModelBase
{
    [ObservableProperty, Serialize] private bool _sidebarExpanded = true;

    [ObservableProperty, Serialize] private double _expandedSidebarWidth = 350;

    [ObservableProperty] private double _currentSidebarWidth;

    public MainControlViewModel(FileNavigatorViewModel fileNavigator)
    {
        FileNavigator = fileNavigator;
        OnExpandedSidebarWidthChanged(_expandedSidebarWidth);
    }

    public FileNavigatorViewModel FileNavigator { get; }

    public IInterfaceData TestDataInterface { get; } = new InterfaceDataTest
    {
        Name = "Test interface",
        Value = (double)2.0,
        Definition = new Slider { Min = 0.0, Max = 1.0 },
    };

    partial void OnSidebarExpandedChanged(bool value)
    {
        CurrentSidebarWidth = value ? ExpandedSidebarWidth : 0;
    }

    partial void OnExpandedSidebarWidthChanged(double value)
    {
        if (SidebarExpanded) CurrentSidebarWidth = value;
    }

    partial void OnCurrentSidebarWidthChanged(double value)
    {
        if (SidebarExpanded) ExpandedSidebarWidth = value;
    }
}

public class InterfaceDataTest : IInterfaceData
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public required string Name { get; init; }
    public required object Value { get; set; }
    public bool IsUserEditable { get; init; }
    public required IUserInterfaceDefinition Definition { get; init; }
}