using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Laminar.Avalonia.Utils;

namespace Laminar.Avalonia.Controls.ScriptEditor.MouseActions;

internal class MouseZoomAction
{
    const double zoomSpeed = 1;

    private readonly IControl _zoomControl;

    public MouseZoomAction(IInputElement zoomListenElement, IControl zoomControl)
    {
        _zoomControl = zoomControl;
        _zoomControl.RenderTransformOrigin = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);
        zoomListenElement.PointerWheelChanged += ZoomListenElement_PointerWheelChanged;

        if (_zoomControl.RenderTransform is null)
        {
            _zoomControl.RenderTransform = new MatrixTransform(Matrix.Identity);
        }
    }

    private void ZoomListenElement_PointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        double zoomAmount = Math.Exp(zoomSpeed * e.Delta.Y / 5);
        Point mouseLocation = e.GetPosition(_zoomControl);
        _zoomControl.RenderTransform = new MatrixTransform(MatrixHelper.ScaleAt(zoomAmount, zoomAmount, mouseLocation.X, mouseLocation.Y) * _zoomControl.RenderTransform.Value);
        _zoomControl.InvalidateVisual();
    }
}
