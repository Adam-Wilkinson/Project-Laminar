using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.DragDrop;

public class DropAcceptor
{
    public bool AcceptDrop(Visual hoverVisual, PointerEventArgs pointerEventArgs, out object? receptacleTag)
    {
        foreach (var receptacle in GetReceptacles(hoverVisual))
        {
            if (receptacle.AcceptsDropRegion.FillContains(pointerEventArgs.GetPosition(hoverVisual.GetVisualParent())))
            {
                receptacleTag = receptacle.Tag;
                return true;
            }
        }

        receptacleTag = null;
        return false;
    }

    public void RenderAllReceptacles(Visual visual, DrawingContext drawingContext)
    {
        if (DebugReceptaclePen is null) return;
        
        foreach (Receptacle receptacle in GetReceptacles(visual))
        {
            receptacle.AcceptsDropRegion.Transform = new MatrixTransform(visual.GetVisualParent()!.TransformToVisual(TopLevel.GetTopLevel(visual)!)!.Value);
            drawingContext.DrawGeometry(null, DebugReceptaclePen, receptacle.AcceptsDropRegion);
        }
    }
    
    protected virtual IPen? DebugReceptaclePen { get; set; }

    protected virtual IEnumerable<Receptacle> GetReceptacles(Visual visual)
    {
        yield return new Receptacle(new RectangleGeometry(visual.Bounds), null);
    }

    protected record struct Receptacle(Geometry AcceptsDropRegion, object? Tag);
}