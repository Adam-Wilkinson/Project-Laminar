using System.Collections.Generic;
using Avalonia;

namespace Laminar.Avalonia.NodeDisplaySystem;

internal class SelectionManager
{
    private readonly List<IObjectFinder> _objectFinders = new();

    public void AddObjectFinder(IObjectFinder objectFinder)
    {
        _objectFinders.Add(objectFinder);
    }

    public IEnumerable<T> GetSelection<T>()
    {
        foreach (IAvaloniaObject aObject in Selection.SelectedObjects)
        {
            if (aObject is T typedValue)
            {
                yield return typedValue;
            }
        }
    }

    public void SelectAtPoint(Point point, bool deselect = false)
    {
        foreach (var objectFinder in _objectFinders)
        {
            objectFinder.GetAtPoint(point)?.SetValue(Selection.SelectedProperty, !deselect);
        }
    }

    public void SelectRectangle(Rect rect, bool deselect = false)
    {
        foreach (var objectFinder in _objectFinders)
        {
            foreach (IAvaloniaObject aObject in objectFinder.GetAllWithin(rect))
            {
                aObject.SetValue(Selection.SelectedProperty, !deselect);
            }
        }
    }

    public void ClearSelection()
    {
        foreach (var objectFinder in _objectFinders)
        {
            foreach (IAvaloniaObject aObject in objectFinder.GetAll())
            {
                aObject.SetValue(Selection.SelectedProperty, false);
            }
        }
    }

    public void SelectAll()
    {
        foreach (var objectFinder in _objectFinders)
        {
            foreach (IAvaloniaObject aObject in objectFinder.GetAll())
            {
                aObject.SetValue(Selection.SelectedProperty, true);
            }
        }
    }
}
