using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.ComponentModel;

namespace Laminar_PluginFramework.Primitives
{
    public interface ILaminarValue : INotifyPropertyChanged
    {
        event EventHandler<ITypeDefinition> TypeDefinitionChanged;

        object Value { get; set; }

        string Name { get; set; }

        ILaminarValue Driver { get; set; }

        IObservableValue<bool> IsUserEditable { get; }

        ITypeDefinitionProvider TypeDefinitionProvider { get; set; }

        ITypeDefinition TypeDefinition { get; }

        bool CanSetValue(object value);

        ILaminarValue Clone();
    }
}