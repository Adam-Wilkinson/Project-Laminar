using System.ComponentModel;

namespace Laminar_PluginFramework.UserInterfaces;

public interface IDisplayValue : INotifyPropertyChanged
{
    public string Name { get; }

    public object Value { get; set; }

    public IUserInterfaceDefinition InterfaceDefinition { get; }
}
