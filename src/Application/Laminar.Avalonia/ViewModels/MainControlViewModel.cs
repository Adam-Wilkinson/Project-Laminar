using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain;

namespace Laminar.Avalonia.ViewModels;

public partial class MainControlViewModel : ViewModelBase, IOpenFileService
{
    private readonly ScopedViewModel<FileNavigatorViewModel> _scopedFileNavigator;
    private readonly FocusedWorkspaceManager _focusedWorkspaceManager;
    private readonly IWorkspaceManager _workspaceManager; 
    
    public MainControlViewModel(
        IServiceProvider serviceProvider,
        IWorkspaceManager workspaceManager,
        FocusedWorkspaceManager focusedWorkspaceManager,
        FileViewModel centralFileEditor)
    {
        _workspaceManager = workspaceManager;
        _scopedFileNavigator = RegisterSubscription(new ScopedViewModel<FileNavigatorViewModel>(serviceProvider, this));
        CentralFileEditor = RegisterSubscription(centralFileEditor);
        _focusedWorkspaceManager = focusedWorkspaceManager;
        
        CentralFileEditor.PropertyChanged += CentralFileEditorOnPropertyChanged;
        _focusedWorkspaceManager.FocusedWorkspaceChanged += OnFocusedWorkspaceChanged;
        _workspaceManager.PluginsChanged += OnPluginsChanged;
        
        OnExpandedSidebarWidthChanged(ExpandedSidebarWidth);
    }

    private void RefreshLoadedNodes()
    {
        if (_focusedWorkspaceManager.FocusedWorkspace?.NodeManager is not { } nodeManager)
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

    protected override void OnDisposed()
    {
        CentralFileEditor.PropertyChanged -= CentralFileEditorOnPropertyChanged;
        _workspaceManager.PluginsChanged -= OnPluginsChanged;
        _focusedWorkspaceManager.FocusedWorkspaceChanged -= OnFocusedWorkspaceChanged;
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

    private void OnFocusedWorkspaceChanged(object? sender, EventArgs e) => RefreshLoadedNodes();

    private void OnPluginsChanged(object? sender, PluginsChangedEventArgs e) => RefreshLoadedNodes();
}