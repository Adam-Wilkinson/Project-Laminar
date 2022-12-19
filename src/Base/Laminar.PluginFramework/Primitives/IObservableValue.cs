using System;
using System.ComponentModel;

namespace Laminar_PluginFramework.Primitives
{
    public interface IObservableValue<T> : INotifyPropertyChanged
    {
        event EventHandler<T> OnChange;

        T Value { get; set; }

        IObservableValue<T> Clone();
    }
}