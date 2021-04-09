using System;
using System.ComponentModel;

namespace OpenFlow_PluginFramework.Primitives
{
    public interface IObservableValue<T> : INotifyPropertyChanged
    {
        T Value { get; set; }

        void AddDependency<TDep>(IObservableValue<TDep> dep, Func<TDep, T> conversion);

        void AddDependency(IObservableValue<T> dep);

        void RemoveDependency<TDep>(IObservableValue<TDep> dep);

        void OnChange(Action<T> changeAction);

        IObservableValue<T> Clone();
    }
}