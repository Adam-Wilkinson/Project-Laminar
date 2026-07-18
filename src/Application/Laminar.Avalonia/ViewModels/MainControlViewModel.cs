using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Laminar.Implementation.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.ViewModels;

public partial class MainControlViewModel : ViewModelBase, IOpenFileService, IDisposable
{
    private readonly ScopedViewModel<FileNavigatorViewModel> _scopedFileNavigator;
    private readonly IScriptingFactory _scriptingFactory;
    private readonly IPersistentDataTranscoder _scriptTranscoder;
    
    public MainControlViewModel(
        IServiceProvider serviceProvider, 
        ILoadedNodeManager loadedNodeManager,
        INodeFactory nodeFactory,
        IScriptingFactory scriptingFactory,
        ILogger<JsonPersistentDataTranscoder> transcoderLogger)
    {
        _scopedFileNavigator = new ScopedViewModel<FileNavigatorViewModel>(serviceProvider, this);
        _scriptingFactory = scriptingFactory;
        _scriptTranscoder = new JsonPersistentDataTranscoder(transcoderLogger);
        CentralFileEditor = new OpenFileViewModel(serviceProvider);
        
        if (FindFirstFile(file => file.CoreItem?.Info.ContentsType == typeof(IScript)) is { } firstScript)
        {
            _ = RequestOpenFile(firstScript);
        }
        
        OnExpandedSidebarWidthChanged(ExpandedSidebarWidth);
        LoadedNodes = loadedNodeManager.LoadedNodes.RecursiveMap(nodeInfo => nodeFactory.FromNodeInfo(nodeInfo));
    }

    public IReadOnlyItemCategory<object> LoadedNodes { get; }

    [Persistent, ObservableProperty] 
    public partial double NodePickerHeight { get; set; } = 250;
    
    [Persistent, ObservableProperty]
    public partial bool SidebarExpanded { get; set; } = true;

    [Persistent, ObservableProperty]
    public partial double ExpandedSidebarWidth { get; set; } = 350;

    [ObservableProperty]
    public partial double CurrentSidebarWidth { get; set; }

    public FileNavigatorViewModel FileNavigator => _scopedFileNavigator.ViewModel;

    public OpenFileViewModel CentralFileEditor { get; }

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

    public Task RequestOpenFile(FileNavigatorItemViewModel file)
    {
        if (file.CoreItem?.Info.ContentsType != typeof(IScript)) return Task.CompletedTask;

        CentralFileEditor.OpenFile(file, _scriptTranscoder, _scriptingFactory,
            (provider, script) => ActivatorUtilities.CreateInstance<ScriptEditorViewModel>(provider, script));

        if (file.CoreItem is IFileSystemFile fileSystemFile)
        {
            FileOpened?.Invoke(this, fileSystemFile);
        }
        
        return Task.CompletedTask;
    }

    public event EventHandler<IFileSystemFile>? FileOpened;
    public event EventHandler<IFileSystemFile>? FileClosed { add { } remove { } }

    public bool FileIsOpen(IFileSystemFile file) => CentralFileEditor.CurrentlyOpenFile == file;

    private FileNavigatorItemViewModel? FindFirstFile(Func<FileNavigatorItemViewModel, bool> predicate)
    {
        FileNavigatorItemViewModel? firstFile = FileNavigator.RootFiles[0];
        while (firstFile.CoreItem is not IFileSystemFile && !predicate(firstFile))
        {
            if (firstFile.Children is null || firstFile.Children.Count == 0)
            {
                firstFile = GetNext(firstFile);
                if (firstFile is null) break;
                continue;
            }

            firstFile = firstFile.Children[0];
        }

        return firstFile;
    }

    private FileNavigatorItemViewModel? GetNext(FileNavigatorItemViewModel? current)
    {
        if (current?.Parent is null) return null;

        var currentItemIndexInParent = current.Parent!.Children!.IndexOf(current); 
        while (currentItemIndexInParent == current.Children!.Count - 1)
        {
            if (current.Parent is null) return null;
            current = current.Parent;
            if (current.Parent is null) return null;
            currentItemIndexInParent = current.Parent.Children!.IndexOf(current);
        }

        return current.Parent!.Children[currentItemIndexInParent + 1];
    }
}