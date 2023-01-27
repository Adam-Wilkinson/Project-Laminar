using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplay : IRefreshable
{
    public IDisplayValue Value { get; }

    public object? Interface { get; }
}
