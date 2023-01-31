using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplay : IRefreshable
{
    public Opacity Opacity { get; }

    public IDisplayValue Value { get; }

    public object? Interface { get; }

    public void KillInterface();
}
