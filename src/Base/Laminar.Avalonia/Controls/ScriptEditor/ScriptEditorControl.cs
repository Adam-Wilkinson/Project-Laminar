using System;
using Avalonia.Input;
using Avalonia.Media;
using Laminar.Avalonia.NodeDisplaySystem;
using Laminar.Contracts.NodeSystem;
using Avalonia.Controls;
using Laminar.Avalonia.Controls.ScriptEditor.MouseActions;
using Laminar.Avalonia.Utils;
using Laminar.Avalonia.Controls.ScriptEditor.ObjectFinders;
using Microsoft.Extensions.DependencyInjection;
using Laminar.Avalonia.KeyBIndings;
using Avalonia;

namespace Laminar.Avalonia.Controls.ScriptEditor;

public class ScriptEditorControl : Border, DragDropHandler.IDragRecepticle
{
    public static readonly StyledProperty<IScript> ActiveScriptProperty = AvaloniaProperty.Register<ScriptEditorControl, IScript>(nameof(ActiveScript));

    private readonly IScriptEditor _scriptEditor = App.LaminarInstance.ServiceProvider.GetService<IScriptEditor>();

    private readonly ConnectionCanvas _displayCanvas = new();
    private readonly NodeControlManager _nodeControlManager = new();
    private readonly SelectionManager _selection = new();
    private readonly MouseZoomAction _zoomAction;
    private IScript _lastScript;

    public ScriptEditorControl() : base()
    {
        _selection.AddObjectFinder(new CanvasChildFinder(_displayCanvas));
        _selection.AddObjectFinder(new ConnectionFinder(_displayCanvas));
        Background = new SolidColorBrush(new Color(0, 0, 0, 0));
        Child = _displayCanvas;
        ClipToBounds = true;
        Focusable = true;
        GestureRecognizers.Add(new MakeConnectionGesture(_nodeControlManager, _displayCanvas, this));
        GestureRecognizers.Add(new MousePanGesture(Child));
        GestureRecognizers.Add(new SelectAndMoveGesture(_displayCanvas, _nodeControlManager, _selection, this));
        GestureRecognizers.Add(new BoxSelectGesture(_selection, _displayCanvas));
        _zoomAction = new MouseZoomAction(this, Child);

        KeyBindings.Add(new LaminarKeyBinding("Delete", new KeyGesture(Key.Delete), DeleteSelection));

        this.GetObservable(ActiveScriptProperty).Subscribe(ScriptChanged);
    }

    public IScript ActiveScript
    {
        get => GetValue(ActiveScriptProperty);
        set => SetValue(ActiveScriptProperty, value);
    }

    public bool EndDrag(DragDropHandler dragDropInstance, PointerReleasedEventArgs e)
    {
        if (dragDropInstance.DragTypeIdentifier == "NodeDisplay" && dragDropInstance.DragContext is INodeWrapper nodeToCopy)
        {
            nodeToCopy.Location.Value = ValueObjectConverter.Point(e.GetPosition(_displayCanvas) - dragDropInstance.ClickOffset);
            _scriptEditor.AddCopyOfNode(ActiveScript, nodeToCopy);
            return true;
        }

        return false;
    }

    private void NodeRemoved(object sender, INodeWrapper node)
    {
        _displayCanvas.Children.Remove(_nodeControlManager.GetControl(node));
        _nodeControlManager.ForgetNode(node);
    }

    private void NodeAdded(object sender, INodeWrapper newNode)
    {
        Control nodeControl = _nodeControlManager.GetControl(newNode);
        Canvas.SetLeft(nodeControl, newNode.Location.Value.X);
        Canvas.SetTop(nodeControl, newNode.Location.Value.Y);
        newNode.Location.PropertyChanged += (o, e) =>
        {
            Canvas.SetLeft(nodeControl, newNode.Location.Value.X);
            Canvas.SetTop(nodeControl, newNode.Location.Value.Y);
        };
        
        _displayCanvas.Children.Add(nodeControl);
    }

    private void DeleteSelection()
    {
        _scriptEditor.UserActionManager.BeginCompoundAction();

        _displayCanvas.DeleteSelection(_selection);

        _scriptEditor.DeleteNodes(ActiveScript, _nodeControlManager.GetSelectedNodes(_selection));

        _scriptEditor.UserActionManager.EndCompoundAction();
    }

    private void ScriptChanged(IScript newScript)
    {
        if (_lastScript is not null)
        {
            _lastScript.Nodes.ItemAdded -= NodeAdded;
            _lastScript.Nodes.ItemRemoved -= NodeRemoved;
        }

        if (newScript is not null)
        {
            newScript.Nodes.ItemAdded += NodeAdded;
            newScript.Nodes.ItemRemoved += NodeRemoved;
            _displayCanvas.Connections = newScript.Connections;
        }

        _lastScript = newScript;
    }
}
