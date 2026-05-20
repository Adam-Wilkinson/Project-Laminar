using System.ComponentModel;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDataInterface<out TFrontend> : INotifyPropertyChanged
{
    public TFrontend InterfaceFrontend { get; }
    
    public IInterfaceData? InterfaceData { get; }
}