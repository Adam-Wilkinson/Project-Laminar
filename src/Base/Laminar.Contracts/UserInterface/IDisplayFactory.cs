using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Contracts.UserInterface;
public interface IDisplayFactory
{
    public IDisplay CreateDisplayForValue(IValueInfo valueInfo);
}
