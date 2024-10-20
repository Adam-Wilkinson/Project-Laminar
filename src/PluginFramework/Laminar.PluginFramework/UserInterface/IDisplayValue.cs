using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface IDisplayValue : INotifyPropertyChanged, ILaminarExecutionSource, IRefreshable
{
    public static readonly PropertyChangedEventArgs ValueChangedEventArgs = new(nameof(Value));

    public string Name { get; }

    public object? Value { get; set; }

    public IUserInterfaceDefinition? InterfaceDefinition { get; }
}
