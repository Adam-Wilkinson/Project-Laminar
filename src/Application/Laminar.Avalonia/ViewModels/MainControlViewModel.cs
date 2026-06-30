using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Laminar.Implementation.Storage.PersistentData;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.ViewModels;

public partial class MainControlViewModel : ViewModelBase, IOpenFileService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ScopedViewModel<FileNavigatorViewModel> _scopedFileNavigator;
    private readonly IScriptingFactory _scriptingFactory;
    private readonly IPersistentDataTranscoder _scriptTranscoder;
    private ScopedViewModel<ScriptEditorViewModel>? _scopedScriptEditor;
    private ILaminarFileResource<IScript>? _openScriptFile;
    
    public MainControlViewModel(
        IServiceProvider serviceProvider, 
        ILoadedNodeManager loadedNodeManager,
        INodeFactory nodeFactory,
        IScriptingFactory scriptingFactory,
        ILogger<JsonPersistentDataTranscoder> transcoderLogger)
    {
        _scopedFileNavigator = new ScopedViewModel<FileNavigatorViewModel>(serviceProvider, this);
        _serviceProvider = serviceProvider;
        
        _scriptingFactory = scriptingFactory;
        _scriptTranscoder = new JsonPersistentDataTranscoder(transcoderLogger);
        
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

    [ObservableProperty]
    public partial ScriptEditorViewModel? ScriptEditor { get; private set; }

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
        _scopedScriptEditor?.Dispose();
        _openScriptFile?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task RequestOpenFile(ILaminarStorageFile file)
    {
        if (file.Info.ContentsType != typeof(IScript)) return Task.CompletedTask;
        
        _openScriptFile?.Dispose();
        _scopedScriptEditor?.Dispose();
        ScriptEditor = null;
        _openScriptFile = file.GetContentsAsResource(_scriptTranscoder, _scriptingFactory);
        _scopedScriptEditor = new ScopedViewModel<ScriptEditorViewModel>(_serviceProvider, _openScriptFile.Value);
        ScriptEditor = _scopedScriptEditor.ViewModel;
        return Task.CompletedTask;
    }

    private ILaminarStorageFile? FindFirstFile(Func<FileNavigatorItemViewModel, bool> predicate)
    {
        FileNavigatorItemViewModel? firstFile = FileNavigator.RootFiles[0];
        while (firstFile.CoreItem is not ILaminarStorageFile && !predicate(firstFile))
        {
            if (firstFile.Children is null || firstFile.Children.Count == 0)
            {
                firstFile = GetNext(firstFile);
                if (firstFile is null) break;
                continue;
            }

            firstFile = firstFile.Children[0];
        }
        
        return firstFile?.CoreItem as ILaminarStorageFile;
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