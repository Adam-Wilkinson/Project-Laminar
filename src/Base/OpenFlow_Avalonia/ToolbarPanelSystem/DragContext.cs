namespace OpenFlow_Avalonia.ToolbarPanelSystem
{
    using Avalonia;

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
}
