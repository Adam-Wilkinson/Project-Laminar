using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplayFactory
{
    public IDisplay CreateDisplayForValue(IValueInfo valueInfo);
}
