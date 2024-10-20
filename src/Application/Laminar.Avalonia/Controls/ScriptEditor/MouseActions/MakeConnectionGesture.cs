using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Laminar.Avalonia.NodeDisplaySystem;
using Laminar.Contracts.Scripting;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.Controls.ScriptEditor.MouseActions;

internal class MakeConnectionGesture : GestureRecogniserBase
{
    private readonly IScriptEditor _scriptEditor = App.LaminarInstance.ServiceProvider.GetService<IScriptEditor>();
    private readonly ScriptEditorControl _scriptEditorControl;
    private readonly Canvas _drawLinesCanvas;
    private readonly NodeControlManager _nodeControlManager;
    private readonly ConnectionLine _currentConnectionLine = new() { StrokeThickness = 2, IsVisible = false };
    private IIOConnector _firstClicked;
    private Control _currentHover;
    private StyledProperty<Point> _activeProperty;

    public MakeConnectionGesture(NodeControlManager nodeControlManager, Canvas drawLinesCanvas, ScriptEditorControl scriptEditorControl)
    {
        _scriptEditorControl = scriptEditorControl;
        _drawLinesCanvas = drawLinesCanvas;
        _nodeControlManager = nodeControlManager;
        _drawLinesCanvas.Children.Add(_currentConnectionLine);
    }

    protected override void TrackedPointerMoved(PointerEventArgs e)
    {
        if (_currentHover is null)
        {
            if (TryGetConnectorAndControlAtPoint(e.GetPosition(_drawLinesCanvas.GetVisualRoot()), out Control controlAtCursor, out IIOConnector connector)
                && _scriptEditor.TryBridgeConnectors(_scriptEditorControl.ActiveScript, _firstClicked, connector))
            {
                _currentConnectionLine.IsVisible = false;
                _currentHover = controlAtCursor;
            }
            else
            {
                _currentConnectionLine.SetValue(_activeProperty, e.GetPosition(_drawLinesCanvas));
            }
        }
        else
        {
            if (!(TryGetConnectorAndControlAtPoint(e.GetPosition(_drawLinesCanvas.GetVisualRoot()), out Control control, out _) && control == _currentHover))
            {
                _scriptEditor.UserActionManager.ResetCompountAction();
                _currentHover = null;
                _currentConnectionLine.SetValue(_activeProperty, e.GetPosition(_drawLinesCanvas));
                _currentConnectionLine.IsVisible = true;
            }
        }
    }

    public override void PointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is Control clickedControl && _nodeControlManager.TryGetConnector(clickedControl, out IIOConnector connector))
        {
            Track(e.Pointer);
            _scriptEditor.UserActionManager.BeginCompoundAction();
            _firstClicked = connector;
            _currentConnectionLine.Stroke = new SolidColorBrush(Color.Parse(connector.ColorHex));
            if (connector is IInputConnector)
            {
                _currentConnectionLine.EndPoint = GetCenterInLocal(clickedControl, clickedControl.Bounds);
                _activeProperty = ConnectionLine.StartPointProperty;
            }
            else if (connector is IOutputConnector)
            {
                _currentConnectionLine.StartPoint = GetCenterInLocal(clickedControl, clickedControl.Bounds);
                _activeProperty = ConnectionLine.EndPointProperty;
            }
            PointerMoved(e);
            _currentConnectionLine.IsVisible = true;
        }
    }

    protected override void EndGesture()
    {
        _currentConnectionLine.IsVisible = false;
        _currentHover = null;
        _scriptEditor.UserActionManager.EndCompoundAction();
    }

    private bool TryGetConnectorAndControlAtPoint(Point point, out Control control, out IIOConnector connector)
    {
        foreach (IVisual visual in _drawLinesCanvas.GetVisualRoot().Renderer.HitTest(point, _drawLinesCanvas.GetVisualRoot(), null))
        {
            if (visual is Control currentControl && _nodeControlManager.TryGetConnector(currentControl, out connector))
            {
                control = currentControl;
                return true;
            }
        }

        control = null;
        connector = null;
        return false;
    }

    private Point GetCenterInLocal(IVisual visual, Rect rect)
        => visual.VisualParent.TranslatePoint(visual.Bounds.Center, _drawLinesCanvas).Value;// + new Point(visual.RenderTransform.Value.M31, visual.RenderTransform.Value.M32);

}
