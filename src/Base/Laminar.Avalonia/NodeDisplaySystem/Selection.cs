using Avalonia;
using System.Collections.Generic;
using System;

namespace Laminar.Avalonia.NodeDisplaySystem;

internal class Selection
{
    public static readonly AttachedProperty<bool> SelectedProperty = AvaloniaProperty.RegisterAttached<Selection, IAvaloniaObject, bool>("Selected");

    private static readonly List<IAvaloniaObject> _selectedObjects = new();

    static Selection()
    {
        SelectedProperty.Changed.Subscribe(x =>
        {
            if (x.NewValue.Value)
            {
                _selectedObjects.Add(x.Sender);
            }
            else
            {
                _selectedObjects.Remove(x.Sender);
            }
        });
    }

    public static void SelectObject(IAvaloniaObject selected)
    {
        if (_selectedObjects.Contains(selected))
        {
            return;
        }

        selected.SetValue(SelectedProperty, true);
    }

    public static void DeselectObject(IAvaloniaObject deselected)
    {
        if (!_selectedObjects.Contains(deselected))
        {
            return;
        }

        deselected.SetValue(SelectedProperty, false);
    }

    public static void Clear()
    {
        while (_selectedObjects.Count > 0)
        {
            DeselectObject(_selectedObjects[0]);
        }
    }

    public static bool IsSelected(IAvaloniaObject selected) => selected.GetValue(SelectedProperty);

    public static IReadOnlyList<IAvaloniaObject> SelectedObjects => _selectedObjects;
}
