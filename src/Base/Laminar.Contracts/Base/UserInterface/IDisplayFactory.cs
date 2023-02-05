using Laminar.PluginFramework.UserInterface;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplayFactory
{
    public IDisplay CreateDisplayForValue(IDisplayValue valueInfo);
}
