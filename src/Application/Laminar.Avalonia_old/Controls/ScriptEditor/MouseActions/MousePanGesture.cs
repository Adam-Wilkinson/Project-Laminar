using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Laminar.Avalonia.Utils;

namespace Laminar.Avalonia.Controls.ScriptEditor.MouseActions;

internal class MousePanGesture : GestureRecogniserBase
{
    readonly IControl _panControl;

    Point _previous;
    Point _pan;

    public MousePanGesture(IControl panControl)
    {
        _panControl = panControl;
        _panControl.RenderTransformOrigin = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);

        if (_panControl.RenderTransform is null)
        {
            _panControl.RenderTransform = new MatrixTransform(Matrix.Identity);
        }
    }

    public override void PointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(_panControl).Properties.IsMiddleButtonPressed)
        {
            Track(e.Pointer);
            _previous = e.GetPosition(_panControl);
            _pan = default;
        }
    }

    protected override void TrackedPointerMoved(PointerEventArgs e)
    {
        Point newLocation = e.GetPosition(_panControl);
        Point delta = newLocation - _previous;
        _previous = newLocation;
        _pan += delta;
        _panControl.RenderTransform = new MatrixTransform(MatrixHelper.Translate(_pan.X, _pan.Y) * _panControl.RenderTransform.Value);
        _panControl.InvalidateVisual();
    }
}
