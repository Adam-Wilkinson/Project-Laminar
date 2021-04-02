namespace OpenFlow_Avalonia.Utils.CloningSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Avalonia.Controls;
    using OpenFlow_Avalonia.Utils.CloningSystem.ObjectCloners;

    public static class Cloner
    {
        internal static readonly Dictionary<Type, IObjectCloner> Cloners = new Dictionary<Type, IObjectCloner>()
        {
            { typeof(TextBox), new TextboxCloner() },
        };

        public static bool TryClone<T>(T toClone, out T newObject)
        {
            if (toClone.GetType().IsValueType)
            {
                newObject = toClone;
                return true;
            }

            if (toClone is ICloneable cloneable)
            {
                newObject = (T)cloneable.Clone();
                return true;
            }

            Debug.WriteLine(toClone.GetType());

            if (Cloners.TryGetValue(toClone.GetType(), out IObjectCloner cloner))
            {
                newObject = (T)cloner.Clone(toClone);
                return true;
            }

            if (toClone.GetType().GetConstructor(Type.EmptyTypes) == null)
            {
                newObject = (T)Activator.CreateInstance(toClone.GetType());
                return false;
            }

            newObject = default;
            return false;
        }
    }
}
