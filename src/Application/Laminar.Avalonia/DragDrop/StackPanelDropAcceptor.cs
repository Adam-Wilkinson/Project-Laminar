using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Laminar.Avalonia.DragDrop;

public class StackPanelDropAcceptor : DropAcceptor<StackPanel>
{
    public double ReceptacleAreaFraction { get; set; } = 0.67;

    public IEnumerable<Receptacle> Receptacles(StackPanel stackPanel) => GetReceptacles(stackPanel);
    
    protected override IEnumerable<Receptacle> GetReceptacles(StackPanel stackPanel)
    {
        if (stackPanel.Children.Count == 0) yield break;
        
        var heightOfFirstReceptacle = ChildDepth(stackPanel, stackPanel.Children[0]) * ReceptacleAreaFraction / 2;
        yield return new Receptacle(
            new RectangleGeometry(OrientedReceptacleRect(stackPanel, 0, heightOfFirstReceptacle)), 0);

        for (var i = 0; i < stackPanel.Children.Count; i++)
        {
            var heightOfReceptacle = ChildDepth(stackPanel, stackPanel.Children[i]) * ReceptacleAreaFraction / 2;
            var receptacleStartPoint =
                (stackPanel.Orientation == Orientation.Horizontal
                    ? stackPanel.Children[i].Bounds.Right
                    : stackPanel.Children[i].Bounds.Bottom)
                - heightOfReceptacle;

            heightOfReceptacle += stackPanel.Spacing;
            
            if (i < stackPanel.Children.Count - 1)
            {
                heightOfReceptacle += ChildDepth(stackPanel, stackPanel.Children[i + 1]) * ReceptacleAreaFraction / 2;
            }

            yield return new Receptacle(
                new RectangleGeometry(OrientedReceptacleRect(stackPanel, receptacleStartPoint, heightOfReceptacle)), i);
        }
    }

    protected override IPen? DebugReceptaclePen { get; set; } = new Pen(new SolidColorBrush((uint)Random.Shared.NextInt64()), 1.5);
    
    private static double ChildDepth(StackPanel stackPanel, Control control)
    {
        return stackPanel.Orientation == Orientation.Horizontal ? control.Bounds.Width : control.Bounds.Height;
    }

    private static Rect OrientedReceptacleRect(StackPanel stackPanel, double startingDepth, double receptacleDepth)
        => stackPanel.Orientation switch
        {
            Orientation.Horizontal => new Rect(startingDepth, 0, receptacleDepth, stackPanel.Bounds.Height),
            _ => new Rect(0, startingDepth, stackPanel.Bounds.Width, receptacleDepth)
        };
}