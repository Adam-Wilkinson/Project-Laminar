using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplay
{
    public IDisplayValue Value { get; }

    public object? Interface { get; }

    public void Refresh();
}
