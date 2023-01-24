using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Laminar.Avalonia.NodeDisplaySystem;

namespace Laminar.Avalonia.Controls.ScriptEditor.MouseActions;

internal class BoxSelectGesture : GestureRecogniserBase
{
    private readonly SelectionManager _selection;
    private readonly Canvas _rectangleDrawCanvas;
    private readonly PointsRectangle _displayRectangle = new() { IsVisible = false, Stroke = new SolidColorBrush(new Color(255, 200, 200, 200)), StrokeThickness = 2 };

    public BoxSelectGesture(SelectionManager selection, Canvas rectangleDrawCanvas)
    {
        _selection = selection;
        _rectangleDrawCanvas = rectangleDrawCanvas;
        _rectangleDrawCanvas.Children.Add(_displayRectangle);
    }

    protected override void TrackedPointerMoved(PointerEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            _selection.ClearSelection();
        }

        _displayRectangle.SecondPoint = e.GetPosition(_rectangleDrawCanvas);
        _selection.SelectRectangle(_displayRectangle.Geometry);
    }

    public override void PointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(Target).Properties.IsLeftButtonPressed)
        {
            Track(e.Pointer);
            _displayRectangle.FirstPoint = e.GetPosition(_rectangleDrawCanvas);
            _displayRectangle.SecondPoint = e.GetPosition(_rectangleDrawCanvas);
            _selection.SelectAtPoint(e.GetPosition(_rectangleDrawCanvas));
            _displayRectangle.IsVisible = true;
        }
    }

    protected override void EndGesture()
    {
        _displayRectangle.IsVisible = false;
    }
}
