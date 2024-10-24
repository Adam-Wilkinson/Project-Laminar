using Avalonia;

namespace Laminar.Avalonia.ToolbarPanelSystem;

internal enum DragMode
{
    None,
    MovingControl,
    ResizingControl,
    ResizingWidth,
}

internal struct DragContext
{
    public DragMode DragMode;
    public SidePanelLayer SidePanelLayer;
    public int ControlIndex;
    public Point InitialClickPoint;
    public ToolbarManager ToolbarGrip;
    public double InitialWidth;
}
