using Laminar.PluginFramework.UserInterface;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplay : IRefreshable
{
    public IDisplayValue DisplayValue { get; }

    public object? Interface { get; }
}
