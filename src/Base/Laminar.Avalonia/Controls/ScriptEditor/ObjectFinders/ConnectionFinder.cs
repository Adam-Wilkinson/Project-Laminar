using System.Collections.Generic;
using System;
using Avalonia;
using Laminar.Avalonia.NodeDisplaySystem;

namespace Laminar.Avalonia.Controls.ScriptEditor.ObjectFinders;

internal class ConnectionFinder : IObjectFinder
{
    private readonly ConnectionCanvas _connectionCanvas;

    public ConnectionFinder(ConnectionCanvas connectionCanvas)
    {
        _connectionCanvas = connectionCanvas;
        Selection.SelectedProperty.Changed.Subscribe(x =>
        {
            if (x.Sender is ConnectionGeometry)
            {
                _connectionCanvas.InvalidateVisual();
            }
        });
    }

    public IEnumerable<IAvaloniaObject> GetAll() => _connectionCanvas.ConnectionGeometries;

    public IEnumerable<IAvaloniaObject> GetAllWithin(Rect rect)
    {
        foreach (var connection in _connectionCanvas.ConnectionGeometries)
        {
            if (connection.Bounds.Intersects(rect))
            {
                yield return connection;
            }
        }
    }

    public IAvaloniaObject? GetAtPoint(Point point)
    {
        foreach (var connection in _connectionCanvas.ConnectionGeometries)
        {
            if (connection.StrokeContains(connection.Pen, point))
            {
                return connection;
            }
        }

        return null;
    }
}
