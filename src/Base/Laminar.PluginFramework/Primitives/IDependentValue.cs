using System;

namespace Laminar_PluginFramework.Primitives;

public interface IDependentValue<T> : IObservableValue<T>
{
    // IObservableValue<bool> HasDependency { get; }

    void SetDependency<TDep>(IObservableValue<TDep> dep, Func<TDep, T> conversion);

    void SetDependency(IObservableValue<T> dep);

    void RemoveDependency<TDep>();
}
