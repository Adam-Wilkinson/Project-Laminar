using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplay : IRefreshable
{
    public IDisplayValue Value { get; }

    public object? Interface { get; }
}
