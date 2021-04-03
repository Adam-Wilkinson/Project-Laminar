using System;
using System.ComponentModel;

namespace OpenFlow_PluginFramework.Primitives
{
    public interface IObservableValue<T> : INotifyPropertyChanged
    {
        T Value { get; set; }

        void AddDependency<TDep>(IObservableValue<TDep> dep);

        void OnChange(Action<T> changeAction);
    }
}