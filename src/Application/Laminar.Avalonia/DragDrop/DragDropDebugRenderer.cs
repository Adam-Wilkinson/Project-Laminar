using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Laminar.Avalonia.DragDrop;

public class DragDropDebugRenderer<T1, T2> : DragDropDebugRenderer
    where T1 : DropAcceptor where T2 : DropAcceptor
{
    protected override bool ShouldDebugControl(Control ctrl)
        => DropTargetHandler.GetDropAcceptor(ctrl) is T1 or T2;
}

public class DragDropDebugRenderer<T> : DragDropDebugRenderer
    where T : DropAcceptor
{
    protected override bool ShouldDebugControl(Control ctrl)
        => DropTargetHandler.GetDropAcceptor(ctrl) is T;
}

public class DragDropDebugRenderer
{
    private readonly Dictionary<Control, RendererControl> _allDebugRenderers = new();

    protected virtual bool ShouldDebugControl(Control ctrl) => true; 
    
    public bool EnsureAttached(Control control)
    {
        if (AdornerLayer.GetAdornerLayer(control) is not { } adornerLayer 
            || _allDebugRenderers.ContainsKey(control)
            || !ShouldDebugControl(control))
        {
            return false;
        }

        RendererControl renderer = new(control);
        adornerLayer.Children.Add(renderer);
        _allDebugRenderers.Add(control, renderer);
        return true;
    }

    public bool Detach(Control control)
    {
        if (AdornerLayer.GetAdornerLayer(control) is not { } adornerLayer 
            || !_allDebugRenderers.TryGetValue(control, out var _))
        {
            return false;
        }

        adornerLayer.Children.Remove(control);
        _allDebugRenderers.Remove(control);
        return true;
    }

    public void EndAll()
    {
        foreach (var control in _allDebugRenderers.Keys)
        {
            Detach(control);
        }
    }

    private class RendererControl : Control
    {
        private readonly Control _debugRenderControl;
        
        public RendererControl(Control control)
        {
            _debugRenderControl = control;
        }
        
        public override void Render(DrawingContext context)
        {
            DropTargetHandler.GetDropAcceptor(_debugRenderControl).RenderAllReceptacles(_debugRenderControl, context);
        }
    }
}