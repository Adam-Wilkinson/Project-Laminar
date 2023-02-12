using System.Collections.Generic;
using Avalonia;

namespace Laminar.Avalonia.NodeDisplaySystem;

internal interface IObjectFinder
{
    public IEnumerable<IAvaloniaObject> GetAtPoint(Point point);

    public IEnumerable<IAvaloniaObject> GetAllWithin(Rect rect);

    public IEnumerable<IAvaloniaObject> GetAll();
}
