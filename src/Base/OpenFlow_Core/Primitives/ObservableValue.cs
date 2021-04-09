using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpenFlow_Core.Primitives
{
    public class ObservableValue<T> : IObservableValue<T>
    {
        private readonly Dictionary<object, object> dependencyFunctions = new();

        private T _value;
        private Action<T> onChange;

        public event PropertyChangedEventHandler PropertyChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (value != null && !value.Equals(_value))
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    onChange?.Invoke(_value);
                }
            }
        }

        public void OnChange(Action<T> changeAction)
        {
            onChange += changeAction;
        }

        public IObservableValue<T> Clone() => new ObservableValue<T>() { Value = Value };

        public void AddDependency<TDep>(IObservableValue<TDep> dep, Func<TDep, T> conversion)
        {
            dependencyFunctions.Add(dep, conversion);
            dep.PropertyChanged += Dep_PropertyChanged<TDep>;
            Value = conversion(dep.Value);
        }

        private void Dep_PropertyChanged<TDep>(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IObservableValue<TDep> val)
            {
                Value = ((Func<TDep, T>)dependencyFunctions[val])(val.Value);
            }
        }

        public void AddDependency(IObservableValue<T> dep)
        {
            AddDependency(dep, x => x);
        }

        public void RemoveDependency<TDep>(IObservableValue<TDep> dep)
        {
            dep.PropertyChanged -= Dep_PropertyChanged<TDep>;
            dependencyFunctions.Remove(dep);
        }
    }
}
