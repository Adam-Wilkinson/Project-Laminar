using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives
{
    public class DependentValue<T> : ObservableValue<T>, IDependentValue<T>
    {
        private object _dependencyFunction;
        private T _valueFromDependency;
        private object _dependency;

        public DependentValue(IObservableValue<bool> hasDependency)
        {
            HasDependency = hasDependency;
            HasDependency.Value = false;
        }

        public override T Value
        {
            get => HasDependency.Value ? _valueFromDependency : base.Value;
            set
            {
                if (!HasDependency.Value)
                {
                    base.Value = value;
                }
            }
        }

        public IObservableValue<bool> HasDependency { get; }

        public void SetDependency(IObservableValue<T> dep)
        {
            SetDependency(dep, x => x);
        }

        public void SetDependency<TDep>(IObservableValue<TDep> dep, Func<TDep, T> conversion)
        {
            _dependencyFunction = conversion;
            _dependency = dep;
            dep.OnChange += OnDependencyChanged;
            HasDependency.Value = true;
            OnDependencyChanged(dep.Value);
        }

        public void RemoveDependency<TDep>()
        {
            (_dependency as IObservableValue<TDep>).OnChange -= OnDependencyChanged;
            _dependencyFunction = null;
            _dependency = null;
            HasDependency.Value = false;
        }

        private void OnDependencyChanged<TDep>(TDep newValue)
        {
            _valueFromDependency = ((Func<TDep, T>)_dependencyFunction)(newValue);
            ValueChanged();
        }
    }
}
