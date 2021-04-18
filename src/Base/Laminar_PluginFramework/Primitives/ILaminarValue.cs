using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.ComponentModel;

namespace Laminar_PluginFramework.Primitives
{
    public interface ILaminarValue : IDependentValue<object>
    {
        event EventHandler<ITypeDefinition> TypeDefinitionChanged;

        string Name { get; set; }

        IObservableValue<bool> IsUserEditable { get; }

        ITypeDefinition TypeDefinition { get; }

        bool CanSetValue(object value);
    }
}