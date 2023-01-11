using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Contracts.Base.UserInterface;
public interface IDisplayFactory
{
    public IDisplay CreateDisplayForValue(IValueInfo valueInfo);
}
