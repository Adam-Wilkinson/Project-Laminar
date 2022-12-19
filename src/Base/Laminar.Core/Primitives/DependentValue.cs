using Laminar_PluginFramework.Primitives;
using System;

namespace Laminar_Core.Primitives
{
    public class DependentValue<T> : ObservableValue<T>, IDependentValue<T>
    {
        private object _dependencyFunction;
        private T _valueFromDependency;
        private object _dependency;

        public override T Value
        {
            get => _dependency is not null ? _valueFromDependency : base.Value;
            set
            {
                if (_dependency is null)
                {
                    base.Value = value;
                }
            }
        }

        public void SetDependency(IObservableValue<T> dep)
        {
            SetDependency(dep, x => x);
        }

        public virtual void SetDependency<TDep>(IObservableValue<TDep> dep, Func<TDep, T> conversion)
        {
            _dependencyFunction = conversion;
            _dependency = dep;
            dep.OnChange += OnDependencyChanged;
            OnDependencyChanged(dep, dep.Value);
        }

        public virtual void RemoveDependency<TDep>()
        {
            (_dependency as IObservableValue<TDep>).OnChange -= OnDependencyChanged;
            _dependencyFunction = null;
            _dependency = null;
            ValueChanged();
        }

        private void OnDependencyChanged<TDep>(object sender, TDep newValue)
        {
            _valueFromDependency = ((Func<TDep, T>)_dependencyFunction)(newValue);
            ValueChanged();
        }
    }
}
