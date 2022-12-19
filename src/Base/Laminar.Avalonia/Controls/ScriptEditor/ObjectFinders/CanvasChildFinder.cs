using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Laminar.Avalonia.NodeDisplaySystem;

namespace Laminar.Avalonia.Controls.ScriptEditor.ObjectFinders;

internal class CanvasChildFinder : IObjectFinder
{
    private readonly Canvas _canvas;

    public CanvasChildFinder(Canvas canvas)
    {
        _canvas = canvas;
    }

    public IEnumerable<IAvaloniaObject> GetAll()
    {
        return _canvas.Children;
    }

    public IEnumerable<IAvaloniaObject> GetAllWithin(Rect rect)
    {
        foreach (var child in _canvas.Children)
        {
            if (child.Bounds.Intersects(rect))
            {
                yield return child;
            }
        }
    }

    public IAvaloniaObject? GetAtPoint(Point point)
    {
        foreach (var child in _canvas.Children)
        {
            if (child.Bounds.Contains(point))
            {
                return child;
            }
        }

        return null;
    }
}
