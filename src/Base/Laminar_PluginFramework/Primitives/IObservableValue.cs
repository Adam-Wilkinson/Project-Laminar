using System;
using System.ComponentModel;

namespace Laminar_PluginFramework.Primitives
{
    public interface IObservableValue<T> : INotifyPropertyChanged
    {
        T Value { get; set; }

        public Action<T> OnChange { get; set; }

        IObservableValue<T> Clone();
    }
}