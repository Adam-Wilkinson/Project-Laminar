using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueInput : IInput
{
    public IValueInterfaceDefinition InterfaceDefinition { get; }

    public IDisplayValue DisplayValue { get; }
}
