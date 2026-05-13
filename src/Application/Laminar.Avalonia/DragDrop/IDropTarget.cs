using System.Threading.Tasks;
using Avalonia;

namespace Laminar.Avalonia.DragDrop;

public interface IDropTarget
{
    public bool HoverEnter(object? payload, Point location, object? receptacleTag);
    
    public bool HoverLeave(object? payload, Point location, object? receptacleTag);
    
    public bool Drop(object? payload, Point location, object? receptacleTag);
}