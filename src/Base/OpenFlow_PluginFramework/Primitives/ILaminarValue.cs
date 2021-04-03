using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System.ComponentModel;

namespace OpenFlow_PluginFramework.Primitives
{
    public interface ILaminarValue : INotifyPropertyChanged
    {
        object Value { get; set; }

        string Name { get; set; }

        ILaminarValue Driver { get; set; }

        bool IsUserEditable { get; set; }

        ITypeDefinitionProvider TypeDefinitionProvider { get; set; }

        ITypeDefinition TypeDefinition { get; }

        bool CanSetValue(object value);

        ILaminarValue Clone();
    }
}