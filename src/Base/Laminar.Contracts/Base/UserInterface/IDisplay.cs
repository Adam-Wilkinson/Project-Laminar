using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDisplay : IRefreshable, INotifyPropertyChanged, ILaminarExecutionSource
{
    public IDisplayValue DisplayValue { get; }

    public object? Interface { get; }
}
