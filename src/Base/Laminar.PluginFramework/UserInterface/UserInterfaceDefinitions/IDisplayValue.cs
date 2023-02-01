using System.ComponentModel;

namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public interface IDisplayValue : INotifyPropertyChanged
{
    public string Name { get; }

    public object? Value { get; set; }

    public IUserInterfaceDefinition InterfaceDefinition { get; }
}
