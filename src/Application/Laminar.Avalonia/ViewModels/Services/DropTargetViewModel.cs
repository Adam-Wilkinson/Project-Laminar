using Avalonia;
using Laminar.Avalonia.DragDrop;

namespace Laminar.Avalonia.ViewModels.Services;

public abstract class DropTargetViewModel : ViewModelBase, IDropTarget
{
    public virtual bool HoverEnter(object? payload, Point location, object? receptacleTag) => false;

    public virtual bool HoverLeave(object? payload, Point location, object? receptacleTag) => false;

    public virtual bool Drop(object? payload, Point location, object? receptacleTag) => false;
}