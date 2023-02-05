using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueOutput : IOutput, IDisplayValue
{
    public ValueInterfaceDefinition ValueUserInterface { get; }
}
