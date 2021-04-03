using OpenFlow_PluginFramework.Primitives;
using System;
using System.ComponentModel;

namespace OpenFlow_Core.Primitives
{
    public class ObservableValue<T> : IObservableValue<T>
    {
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

        public void AddDependency<TDep>(IObservableValue<TDep> dep)
        {
            dep.PropertyChanged += (o, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            };
        }

        public void OnChange(Action<T> changeAction)
        {
            onChange += changeAction;
        }
    }
}
