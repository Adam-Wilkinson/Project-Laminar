using System.Collections.Specialized;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.SelectAndMove;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.NodeSystem.Connectors;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;
using AvaloniaPoint = Avalonia.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorViewModel(
    IScript script, 
    IScriptEditor editor, 
    IUserActionManager userActionManager,
    IEncodableDataFactory dataFactory,
    IScriptingFactory scriptingFactory,
    TopLevel topLevel)
    : DropTargetViewModel, IConnectionInteractionHandler, IClipboardProvider
{
    private static readonly IPersistentDataTranscoder DefaultClipboardTranscoder = new JsonPersistentDataTranscoder(null!); 
    
    private readonly Dictionary<object, ScriptEditorItemModel> _itemModels = [];
    private FlattenedObservableTree<ScriptEditorItemModel>? _models;
    
    private IUserActionSession? _userActionSession;

    [ObservableProperty]
    public partial CanvasSelectionModel? SelectionModel { get; set; }
    
    public IReadOnlyObservableCollection<ScriptEditorItemModel> VisualElements 
        => _models ??= new FlattenedObservableTree<ScriptEditorItemModel>(
                script.WritableNodeTree.Nodes.ObservableMap(CreateItemModel),
                script.WritableNodeTree.Connections.ObservableMap(CreateItemModel));
    
    public override bool Drop(object? payload, AvaloniaPoint location, object? receptacleTag)
    {
        if (payload is not IWrappedNode wrapped) return false;

        var addNodeAction =
            editor.AddMatchingNodeAction(script, wrapped, new LaminarPoint { X = location.X, Y = location.Y });
        userActionManager.ExecuteAction(addNodeAction);
        return true;
    }

    public IConnector? StartConnectionFrom(IConnector connector)
    {
        if (connector.Flags.HasFlag(ConnectorFlags.AcceptsConnections))
        {
            _userActionSession = userActionManager.BeginSession();
            return connector;            
        }

        if (connector.Flags == (ConnectorFlags.HasConnections | ConnectorFlags.ConnectionsSaturated))
        {
            var connections = script.WritableNodeTree.GetConnectionsTo(connector);
            if (connections.Count == 0) return null;
            var connectionInfo = connections.First();

            _userActionSession ??= userActionManager.BeginSession();
            _userActionSession.ExecuteAction(editor.DeleteConnectionAction(script, connectionInfo.Connection));

            return connectionInfo.OppositeConnector;
        }

        return null;
    }

    public bool HoverConnection(IConnector first, IConnector second)
    {
        _userActionSession ??= userActionManager.BeginSession();

        if (editor.FindBridgeConnectorsAction(script, first, second) is not { } bridgeAction) return false;
        
        _userActionSession.ExecuteAction(bridgeAction);
        return true;

    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelection))]
    private void DeleteSelection()
    {
        if (SelectionModel is null || SelectionModel.SelectedItems.Count == 0) return;

        using var session = userActionManager.BeginSession();
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
                     .OfType<IWrappedNode>()
                     .ToList())
        {
            session.ExecuteAction(editor.DeleteNodeAction(script, connection));
        }
    }

    public bool CanDeleteSelection => SelectionModel is not null && SelectionModel.SelectedItems.Count > 0; 

    public void CancelConnection()
    {
        _userActionSession?.Pop();
    }

    public void ConfirmConnection()
    {
        _userActionSession?.Dispose();
        _userActionSession = null;
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
        if (!CanCopyToClipboard || topLevel.Clipboard is not { } clipboard) return;

        List<IWrappedNode> selectedNodes = [];
        List<IConnection> selectedConnections = [];
        
        foreach (var selected in SelectionModel?.SelectedItems.Cast<ScriptEditorItemModel>() ?? [])
        {
            switch (selected.CoreElement)
            {
                case IWrappedNode wrappedNode:
                    selectedNodes.Add(wrappedNode);
                    break;
                case IConnection connection:
                    selectedConnections.Add(connection);
                    break;
            }
        }
        
        var encodedNodeTree = scriptingFactory
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
        if (!CanCopyToClipboard || topLevel.Clipboard is null) return;
        await CopyToClipboard();
        DeleteSelection();
    }
    
    [RelayCommand]
    private async Task PasteFromClipboard()
    {
        if (topLevel.Clipboard is not { } clipboard) return;

        var result = await clipboard.TryGetDataAsync();
        if (result is null) return;

        SelectionModel?.DeselectAll();
        VisualElements.CollectionChanged += SelectNewItems;

        foreach (var transferItem in result.Items)
        {
            var stringResult = await transferItem.TryGetTextAsync();
            if (stringResult is null) continue;
            var dictionary = dataFactory.GetEncodableData<IPersistentDictionary>();
            dictionary.Decode(DefaultClipboardTranscoder, DefaultClipboardTranscoder.BytesToElement(Encoding.UTF8.GetBytes(stringResult))!);
            var deserializedNodeTree = scriptingFactory.NodeTreeFromPersistentData(dictionary);
            var pasteAction = editor.AddSubTree(script, deserializedNodeTree);
            await userActionManager.ExecuteAction(pasteAction);
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

    public bool CanCopyToClipboard => SelectionModel is not null && SelectionModel.SelectedItems.Cast<ScriptEditorItemModel>().Any(x => x.CoreElement is IWrappedNode);

    private ScriptEditorItemModel CreateItemModel(object target)
    {
        var output = target switch
        {
            IConnection connection => new ScriptEditorItemModel(connection),
            IWrappedNode node => new ScriptEditorItemModel(node),
            not null => throw new InvalidOperationException($"Unknown script editor item model {target}"),
            null => throw new ArgumentNullException(nameof(target))
        };
        
        _itemModels[target] = output;
        return output;
    }
}