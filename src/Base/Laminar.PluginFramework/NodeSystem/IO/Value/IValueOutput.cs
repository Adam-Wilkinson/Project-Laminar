using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.IO.Value;

public interface IValueOutput : IOutput
{
    public IValueInterfaceDefinition InterfaceDefinition { get; }

    public IDisplayValue DisplayValue { get; }
}
