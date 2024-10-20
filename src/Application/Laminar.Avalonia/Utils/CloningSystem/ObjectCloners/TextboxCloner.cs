using System;
using Avalonia.Controls;

namespace Laminar.Avalonia.Utils.CloningSystem.ObjectCloners;

public class TextboxCloner : IObjectCloner
{
    public object Clone(object toClone)
    {
        if (toClone == null)
        {
            throw new ArgumentNullException(nameof(toClone));
        }

        return new TextBox()
        {
            Text = ((TextBox)toClone).Text,
        };
    }
}
