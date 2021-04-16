namespace Laminar_Avalonia.Utils.CloningSystem.ObjectCloners
{
    using System;
    using Avalonia.Controls;

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
}
