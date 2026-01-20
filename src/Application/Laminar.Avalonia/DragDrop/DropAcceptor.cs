using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.DragDrop;

public class DropAcceptor
{
    public Receptacle? AcceptDrop(Visual hoverVisual, PointerEventArgs pointerEventArgs)
    {
        var pointerPosition = pointerEventArgs.GetPosition(hoverVisual);
        foreach (var currentReceptacle in GetReceptacles(hoverVisual))
        {
            if (currentReceptacle.AcceptsDropRegion.FillContains(pointerPosition))
            {
                return currentReceptacle;
            }
        }
        
        return null;
    }

    public void RenderAllReceptacles(Visual visual, DrawingContext drawingContext)
    {
        if (DebugReceptaclePen is null) return;
        if (TopLevel.GetTopLevel(visual) is not { } topLevel) return;
        if (visual.GetVisualParent()?.TransformToVisual(topLevel) is not { } matrixToTopLevel) return;

        Transform transformToTopLevel = new MatrixTransform(matrixToTopLevel);
        
        foreach (var receptacle in GetReceptacles(visual))
        {
            receptacle.AcceptsDropRegion.Transform = transformToTopLevel;
            drawingContext.DrawGeometry(null, DebugReceptaclePen, receptacle.AcceptsDropRegion);
        }
    }
    
    protected virtual IPen? DebugReceptaclePen { get; set; }
    
    protected virtual IEnumerable<Receptacle> GetReceptacles(Visual visual)
    {
        yield return new Receptacle(new RectangleGeometry(visual.Bounds), null);
    }

    public record struct Receptacle(Geometry AcceptsDropRegion, object? Tag);
}