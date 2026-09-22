using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Avalonia.Input;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.DragDrop;
using Laminar.Avalonia.SelectAndMove;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.NodeSystem.Connectors;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;
using AvaloniaPoint = Avalonia.Point;
using UndoRedoHandler = Laminar.Avalonia.ViewModels.Contracts.UndoRedoHandler;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel(
    IScript script, 
    IScriptEditor editor,
    IEncodableDataFactory dataFactory,
    Option<IClipboard> optionalClipboard)
    : ViewModelBase(script), IDropTarget, IUndoRedoScope, IConnectionInteractionHandler, IClipboardProvider
{
    private static readonly IPersistentDataTranscoder DefaultClipboardTranscoder = new JsonPersistentDataTranscoder(null!); 
    
    private readonly Dictionary<object, ScriptEditorItemModel> _itemModels = [];
    
    private BoundObservableCollection<ScriptEditorItemModel>? _models;
    private IUserActionSession? _userActionSession;

    [ObservableProperty]
    public partial CanvasSelectionModel? SelectionModel { get; set; }

    [ObservableProperty] 
    public partial double PanX { get; set; } = script.Pan.Value.X;
    [ObservableProperty] 
    public partial double PanY { get; set; } = script.Pan.Value.Y;
    
    public IObservableValue<double> Zoom { get; } = script.Zoom;

    public IRuntimeHost RuntimeHost => script.Runtime;

    public UndoRedoHandler UndoRedo { get; } = new(script.ActionScope);
    
    public IReadOnlyObservableCollection<ScriptEditorItemModel> VisualElements 
        => _models ??= new BoundObservableCollection<ScriptEditorItemModel>(GetVisualElements());

    protected override void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IScript.NodeGraph))
        {
            _models?.BindTo(GetVisualElements());
        }
    }

    public bool HoverEnter(object? payload, AvaloniaPoint location, object? receptacleTag) => false;

    public bool HoverLeave(object? payload, AvaloniaPoint location, object? receptacleTag) => false;

    public bool Drop(object? payload, AvaloniaPoint location, object? receptacleTag)
    {
        if (payload is not INodeContainer wrapped) return false;

        var addNodeAction =
            editor.AddMatchingNodeAction(script, wrapped, new LaminarPoint { X = location.X, Y = location.Y });
        script.ActionScope.ExecuteAction(addNodeAction);
        return true;
    }

    public IConnector? StartConnectionFrom(IConnector connector)
    {
        if (connector.Flags.HasFlag(ConnectorFlags.AcceptsConnections))
        {
            _userActionSession = script.ActionScope.BeginSession();
            return connector;            
        }

        if (connector.Flags == (ConnectorFlags.HasConnections | ConnectorFlags.ConnectionsSaturated))
        {
            var connections = script.NodeGraph.GetConnectionsTo(connector);
            if (connections.Count == 0) return null;
            var connectionInfo = connections.First();

            _userActionSession ??= script.ActionScope.BeginSession();
            _userActionSession.ExecuteAction(editor.DeleteConnectionAction(script, connectionInfo.Connection));
            
            return connectionInfo.OppositeConnector;
        }

        return null;
    }

    public bool HoverConnection(IConnector first, IConnector second)
    {
        _userActionSession ??= script.ActionScope.BeginSession();

        if (script.NodeGraph.ConnectionExists(first, second, out _)) return false;
        
        if (editor.FindBridgeConnectorsAction(script, first, second) is not { } bridgeAction) return false;

        _userActionSession.ExecuteAction(bridgeAction);
        return true;

    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelection))]
    private void DeleteSelection()
    {
        if (SelectionModel is null || SelectionModel.SelectedItems.Count == 0) return;

        using var session = script.ActionScope.BeginSession();
        foreach (var connection in SelectionModel.SelectedItems
                     .Cast<ScriptEditorItemModel>()
                     .Select(x => x.CoreElement)
                     .OfType<IConnection>()
                     .ToList())
        {
            session.ExecuteAction(editor.DeleteConnectionAction(script, connection));
        }

        foreach (var connection in SelectionModel.SelectedItems
                     .Cast<ScriptEditorItemModel>()
                     .Select(x => x.CoreElement)
                     .OfType<INodeContainer>()
                     .ToList())
        {
            session.ExecuteAction(editor.DeleteNodeAction(script, connection));
        }
    }

    public bool CanDeleteSelection => SelectionModel is not null && SelectionModel.SelectedItems.Count > 0; 

    public void CancelCurrentConnection()
    {
        _userActionSession?.Pop();
    }

    public void ConfirmCurrentConnection()
    {
        _userActionSession?.Dispose();
        _userActionSession = null;
    }

    public void ExitInteraction()
    {
        _userActionSession?.Reset();
        ConfirmCurrentConnection();
    }

    partial void OnSelectionModelChanged(CanvasSelectionModel? oldValue, CanvasSelectionModel? newValue)
    {
        oldValue?.ItemDeselected -= OnDeselection;
        oldValue?.ItemSelected -= OnSelection;
        newValue?.ItemDeselected += OnDeselection;
        newValue?.ItemSelected += OnSelection;
    }

    private void OnSelection(object? sender, CanvasSelectionModel.ItemSelectedEventArgs e)
    {
        OnCurrentSelectionChanged();
    }

    private void OnDeselection(object? sender, CanvasSelectionModel.ItemDeselectedEventArgs e)
    {
        OnCurrentSelectionChanged();
    }
    
    private void OnCurrentSelectionChanged()
    {
        OnPropertyChanged(nameof(CanDeleteSelection));
        OnPropertyChanged(nameof(CanCopyToClipboard));
    }

    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task CopyToClipboard()
    {
        if (!CanCopyToClipboard || optionalClipboard.Value is not { } clipboard) return;

        List<INodeContainer> selectedNodes = [];
        List<IConnection> selectedConnections = [];
        
        foreach (var selected in SelectionModel?.SelectedItems.Cast<ScriptEditorItemModel>() ?? [])
        {
            switch (selected.CoreElement)
            {
                case INodeContainer wrappedNode:
                    selectedNodes.Add(wrappedNode);
                    break;
                case IConnection connection:
                    selectedConnections.Add(connection);
                    break;
            }
        }
        
        var encodedNodeTree = script.Runtime.ScriptingFactory
            .CreateNodeTree(selectedNodes, selectedConnections)
            .PersistentData
            .Encode(DefaultClipboardTranscoder);
        
        var transfer = new DataTransfer();
        transfer.Add(DataTransferItem.CreateText(Encoding.UTF8.GetString(DefaultClipboardTranscoder.ElementToBytes(encodedNodeTree))));
        await clipboard.SetDataAsync(transfer);
    }

    [RelayCommand(CanExecute = nameof(CanCopyToClipboard))]
    private async Task Cut()
    {
        if (!CanCopyToClipboard || optionalClipboard.Value is null) return;
        await CopyToClipboard();
        DeleteSelection();
    }
    
    [RelayCommand]
    private async Task PasteFromClipboard()
    {
        if (optionalClipboard.Value is not { } clipboard) return;

        var result = await clipboard.TryGetDataAsync();
        if (result is null) return;

        SelectionModel?.DeselectAll();
        VisualElements.CollectionChanged += SelectNewItems;

        foreach (var transferItem in result.Items)
        {
            var stringResult = await transferItem.TryGetTextAsync();
            if (string.IsNullOrWhiteSpace(stringResult)) continue;
            var dictionary = dataFactory.GetEncodableData<IPersistentDictionary>();
            dictionary.Decode(DefaultClipboardTranscoder, DefaultClipboardTranscoder.BytesToElement(Encoding.UTF8.GetBytes(stringResult))!);
            var deserializedNodeTree = script.Runtime.ScriptingFactory.NodeTreeFromPersistentData(dictionary);
            var pasteAction = editor.AddSubTree(script, deserializedNodeTree);
            await script.ActionScope.ExecuteAction(pasteAction);
        }
        
        VisualElements.CollectionChanged -= SelectNewItems;
        
        return;
        
        void SelectNewItems(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null) return;
            
            foreach (var element in e.NewItems.Cast<ScriptEditorItemModel>())
            {
                element.IsSelected = true;
            }
        }
    }

    public bool CanCopyToClipboard => SelectionModel is not null && SelectionModel.SelectedItems.Cast<ScriptEditorItemModel>().Any(x => x.CoreElement is INodeContainer);

    partial void OnPanXChanged(double value)
    {
        script.Pan.Value = new LaminarPoint { X = PanX, Y = PanY };
    }

    partial void OnPanYChanged(double value)
    {
        script.Pan.Value = new LaminarPoint { X = PanX, Y = PanY };
    }

    private IReadOnlyObservableCollection<ScriptEditorItemModel> GetVisualElements()
        => new FlattenedObservableTree<ScriptEditorItemModel>(
            script.NodeGraph.Nodes.ObservableMap(CreateItemModel),
            script.NodeGraph.Connections.ObservableMap(CreateItemModel));
    
    private ScriptEditorItemModel CreateItemModel(object target)
    {
        var output = target switch
        {
            IConnection connection => new ScriptEditorItemModel(connection),
            INodeContainer node => new ScriptEditorItemModel(node),
            not null => throw new InvalidOperationException($"Unknown script editor item model {target}"),
            null => throw new ArgumentNullException(nameof(target))
        };
        
        _itemModels[target] = output;
        return output;
    }
}