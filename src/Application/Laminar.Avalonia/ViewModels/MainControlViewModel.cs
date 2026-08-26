using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain;

namespace Laminar.Avalonia.ViewModels;

public partial class MainControlViewModel : ViewModelBase, IOpenFileService, IDisposable
{
    private readonly ScopedViewModel<FileNavigatorViewModel> _scopedFileNavigator;
    private readonly FocusedRuntimeManager _focusedRuntimeManager;
    
    public MainControlViewModel(
        IServiceProvider serviceProvider, 
        IRuntimeHostManager runtimeHostManager,
        FocusedRuntimeManager focusedFocusedRuntimeManager,
        FileViewModel centralFileEditor)
    {
        _scopedFileNavigator = new ScopedViewModel<FileNavigatorViewModel>(serviceProvider, this);
        _focusedRuntimeManager = focusedFocusedRuntimeManager;
        CentralFileEditor = centralFileEditor;
        
        CentralFileEditor.PropertyChanged += CentralFileEditorOnPropertyChanged;
        OnExpandedSidebarWidthChanged(ExpandedSidebarWidth);
        runtimeHostManager.PluginsChanged += (_, _) => RefreshLoadedNodes();
        focusedFocusedRuntimeManager.FocusedRuntimeChanged += (_, _) => RefreshLoadedNodes();
    }

    private void RefreshLoadedNodes()
    {
        if (_focusedRuntimeManager.FocusedRuntime?.NodeManager is not { } nodeManager)
        {
            LoadedNodes = null;
            return;
        }
        
        LoadedNodes = nodeManager.LoadedNodes.RecursiveMap(nodeManager.CreateNode);
    }

    [ObservableProperty]
    public partial IReadOnlyItemCategory<object>? LoadedNodes { get; private set; }

    [Persistent, ObservableProperty] 
    public partial double NodePickerHeight { get; set; } = 250;
    
    [Persistent, ObservableProperty]
    public partial bool SidebarExpanded { get; set; } = true;

    [Persistent, ObservableProperty]
    public partial double ExpandedSidebarWidth { get; set; } = 350;

    [ObservableProperty]
    public partial double CurrentSidebarWidth { get; set; }

    public FileNavigatorViewModel FileNavigator => _scopedFileNavigator.ViewModel;

    public FileViewModel CentralFileEditor { get; }

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
        CentralFileEditor.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task RequestOpenFile(IFileSystemFile newFile)
    {
        if (newFile.Info.ContentsType != typeof(IScript)) return Task.CompletedTask;
        
        CentralFileEditor.OpenFilePath = newFile.Path;

        return Task.CompletedTask;
    }

    public event EventHandler? OpenFilesChanged;

    public bool FileIsOpen(IFileSystemFile file) => Equals(CentralFileEditor.CurrentFile, file);
    
    private void CentralFileEditorOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CentralFileEditor.CurrentFile))
        {
            OpenFilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}