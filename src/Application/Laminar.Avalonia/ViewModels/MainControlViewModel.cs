using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;

namespace Laminar.Avalonia.ViewModels;

public partial class MainControlViewModel : ViewModelBase, IDisposable
{
    private readonly ScopedViewModel<FileNavigatorViewModel> _scopedFileNavigator;
    
    public MainControlViewModel(IServiceProvider serviceProvider, NodePickerViewModel nodePicker)
    {
        _scopedFileNavigator = new ScopedViewModel<FileNavigatorViewModel>(serviceProvider);
        NodePicker = nodePicker;
        OnExpandedSidebarWidthChanged(ExpandedSidebarWidth);
    }

    public NodePickerViewModel NodePicker { get; }
    
    [Persistent, ObservableProperty]
    public partial bool SidebarExpanded { get; set; } = true;

    [Persistent, ObservableProperty]
    public partial double ExpandedSidebarWidth { get; set; } = 350;

    [ObservableProperty]
    public partial double CurrentSidebarWidth { get; set; }

    public FileNavigatorViewModel FileNavigator => _scopedFileNavigator.ViewModel;

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

    public void Dispose()
    {
        _scopedFileNavigator.Dispose();
        GC.SuppressFinalize(this);
    }
}