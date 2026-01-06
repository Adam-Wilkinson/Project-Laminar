using System;
using System.Collections.Generic;
using Avalonia;

namespace Laminar.Avalonia.DragDrop;

public abstract class DropAcceptor<T> : DropAcceptor where T : Visual
{
    protected abstract IEnumerable<Receptacle> GetReceptacles(T visual);
    
    protected sealed override IEnumerable<Receptacle> GetReceptacles(Visual visual)
    {
        if (visual is not T typedVisual)
        {
            throw new ArgumentException($@"Visual must be of type {typeof(T)}", nameof(visual));
        }

        return GetReceptacles(typedVisual);
    }
}