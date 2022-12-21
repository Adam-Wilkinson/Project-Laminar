using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Contracts.UserInterface;

public interface IDisplay
{
    public IDisplayValue Value { get; }

    public object? Interface { get; }

    public void Refresh();
}
