using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Laminar.Avalonia.NodeDisplaySystem;
using Laminar.Avalonia.Utils;
using Laminar.Contracts.NodeSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.Controls.ScriptEditor.MouseActions;

internal class SelectAndMoveGesture : GestureRecogniserBase
{
    private readonly IScriptEditor _scriptEditor = App.LaminarInstance.ServiceProvider.GetService<IScriptEditor>();
    private readonly ScriptEditorControl _scriptEditorControl;
    private readonly SelectionManager _selection;
    private readonly NodeControlManager _nodeControlManager;
    private readonly Canvas _positionCanvas;
    private Point _gestureStartPoint;
    private Point _totalMoveDistance;
    private Control _clickedControl;

    public SelectAndMoveGesture(Canvas positionCanvas, NodeControlManager nodeControlManager, SelectionManager nodeSelection, ScriptEditorControl scriptEditor)
    {
        _scriptEditorControl = scriptEditor;
        _selection = nodeSelection;
        _nodeControlManager = nodeControlManager;
        _positionCanvas = positionCanvas;

        _nodeControlManager.NodeClicked += NodeControlManager_NodeClicked;
    }

    private void NodeControlManager_NodeClicked(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(Target).Properties.IsLeftButtonPressed)
        {
            Track(e.Pointer);
            _clickedControl = (Control)sender;
        }
    }

    protected override void TrackedPointerMoved(PointerEventArgs e)
    {
        _totalMoveDistance = e.GetPosition(_positionCanvas) - _gestureStartPoint;
        foreach (Control control in _selection.GetSelection<Control>())
        {
            if (_nodeControlManager.TryGetNode(control, out INodeWrapper node))
            {
                Canvas.SetLeft(control, node.Location.Value.X + _totalMoveDistance.X);
                Canvas.SetTop(control, node.Location.Value.Y + _totalMoveDistance.Y);
            }
        }
    }

    public override void PointerPressed(PointerPressedEventArgs e)
    {
        if (e.Pointer == Pointer)
        {
            if (Selection.IsSelected(_clickedControl) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                _selection.SelectAtPoint(e.GetPosition(_positionCanvas), true);
                Pointer = null;
            }
            else
            {
                if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) && !Selection.IsSelected(_clickedControl))
                {
                    _selection.ClearSelection();
                }

                _selection.SelectAtPoint(e.GetPosition(_positionCanvas));
            }

            _gestureStartPoint = e.GetPosition(_positionCanvas);
        }
        else
        {
            _selection.ClearSelection();
        }
    }

    protected override void EndGesture()
    {
        _scriptEditor.MoveNodes(_scriptEditorControl.ActiveScript, _nodeControlManager.GetSelectedNodes(_selection), ValueObjectConverter.Point(_totalMoveDistance));
    }
}
