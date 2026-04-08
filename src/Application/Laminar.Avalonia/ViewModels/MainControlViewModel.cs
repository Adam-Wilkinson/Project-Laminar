using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;

namespace Laminar.Avalonia.ViewModels;

public partial class MainControlViewModel : ViewModelBase, IDisposable
{
    private readonly ScopedViewModel<FileNavigatorViewModel> _scopedFileNavigator;
    
    [ObservableProperty]
    [field: Serialize]
    public partial bool SidebarExpanded { get; set; } = true;

    [ObservableProperty]
    [field: Serialize]
    public partial double ExpandedSidebarWidth { get; set; } = 350;

    [ObservableProperty]
    public partial double CurrentSidebarWidth { get; set; }

    public MainControlViewModel(IServiceProvider serviceProvider)
    {
        _scopedFileNavigator = new ScopedViewModel<FileNavigatorViewModel>(serviceProvider);
        OnExpandedSidebarWidthChanged(ExpandedSidebarWidth);
    }

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