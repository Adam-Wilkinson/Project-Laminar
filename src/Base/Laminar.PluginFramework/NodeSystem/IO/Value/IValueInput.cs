using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueInput : IInput, IDisplayValue
{
    public ValueInterfaceDefinition ValueUserInterface { get; }
}
