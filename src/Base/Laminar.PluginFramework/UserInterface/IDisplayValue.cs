using System.ComponentModel;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface IDisplayValue : IRefreshable, INotifyPropertyChanged
{
    public static readonly PropertyChangedEventArgs ValueChangedEventArgs = new(nameof(Value));

    public string Name { get; }

    public object? Value { get; set; }

    public IUserInterfaceDefinition? InterfaceDefinition { get; }
}
