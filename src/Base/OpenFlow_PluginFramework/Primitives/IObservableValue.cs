using System.ComponentModel;

namespace OpenFlow_PluginFramework.Primitives
{
    public interface IObservableValue<T> : INotifyPropertyChanged
    {
        T Value { get; set; }
    }
}