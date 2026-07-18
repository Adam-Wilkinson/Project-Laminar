using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Laminar.Domain.ValueObjects;
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

        _ = InitializeOpenFile();
        
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

    [Persistent, ObservableProperty]
    public partial FileSystemPath? OpenFilePath { get; set; }

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

    public Task RequestOpenFile(IFileSystemFile file)
    {
        if (file.Info.ContentsType != typeof(IScript)) return Task.CompletedTask;

        if (CentralFileEditor.CurrentlyOpenFile is { } currentlyOpenFile)
        {
            CentralFileEditor.Close();
            OpenFilePath = null;
            FileClosed?.Invoke(this, currentlyOpenFile);
        }
        
        CentralFileEditor.OpenFile(file, _scriptTranscoder, _scriptingFactory,
            (provider, script) => ActivatorUtilities.CreateInstance<ScriptEditorViewModel>(provider, script));

        OpenFilePath = file.Path;
        FileOpened?.Invoke(this, file);
        
        return Task.CompletedTask;
    }

    public event EventHandler<IFileSystemFile>? FileOpened;
    public event EventHandler<IFileSystemFile>? FileClosed;

    public bool FileIsOpen(IFileSystemFile file) => OpenFilePath == file.Path;
    
    private async Task InitializeOpenFile()
    {
        if (await FindFirstFileSystemItem(ShouldOpenFile) is IFileSystemFile fileToOpen)
        {
            await RequestOpenFile(fileToOpen);
        }
        
        return;
        bool ShouldOpenFile(IFileSystemItem item) =>
            (OpenFilePath is null || item.Path == OpenFilePath)
            && item is IFileSystemFile
            && item.Info.ContentsType == typeof(IScript);
    }
    
    private async Task<IFileSystemItem?> FindFirstFileSystemItem(Func<IFileSystemItem, bool> predicate)
    {
        IFileSystemItem? currentItem = FileNavigator.RootFiles[0].CoreItem;
        if (currentItem is null) return null;
        while (!predicate(currentItem))
        {
            if (currentItem is not IFileSystemFolder currentFolder || (await currentFolder.GetOrLoadContentsAsync()).Count == 0)
            {
                var nextFile = GetNext(currentItem);
                if (nextFile is null) break;
                currentItem = nextFile;
                continue;
            }

            currentItem = (await currentFolder.GetOrLoadContentsAsync())[0];
        }

        return currentItem;
    }

    private IFileSystemItem? GetNext(IFileSystemItem? current)
    {
        if (current?.ParentFolder is null) return null;

        var currentItemIndexInParent = current.ParentFolder!.Contents!.IndexOf(current);
        
        // We step up in the tree while the current item is the last item
        while (currentItemIndexInParent == current.ParentFolder!.Contents!.Count - 1)
        {
            if (current.ParentFolder is null) return null;
            current = current.ParentFolder;
            
            if (current is IFileSystemRootFolder rootFolder)
            {
                var indexOfRootFolder = FileNavigator.RootFiles.Index().First(x => x.Item.CoreItem == rootFolder).Index;
                if (indexOfRootFolder == -1 || indexOfRootFolder >= FileNavigator.RootFiles.Count - 1) return null;
                return FileNavigator.RootFiles[indexOfRootFolder + 1].CoreItem;
            }
            
            if (current.ParentFolder is null) return null;
            currentItemIndexInParent = current.ParentFolder.Contents!.IndexOf(current);
        }

        return current.ParentFolder!.Contents[currentItemIndexInParent + 1];
    }
}