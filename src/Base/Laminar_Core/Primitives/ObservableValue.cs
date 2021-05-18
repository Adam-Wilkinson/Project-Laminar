using Laminar_PluginFramework.Primitives;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Laminar_Core.Primitives
{
    public class ObservableValue<T> : IObservableValue<T>
    {
        private T _value;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<T> OnChange;

        public virtual T Value
        {
            get => _value;
            set
            {
                if (value is not null && !value.Equals(_value))
                {
                    _value = value;
                    ValueChanged();
                }
            }
        }

        public virtual IObservableValue<T> Clone() => new ObservableValue<T>() { Value = Value };

        public override string ToString()
        {
            return $"{Value} (Observable)";
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void ValueChanged()
        {
            NotifyPropertyChanged(nameof(Value));
            OnChange?.Invoke(this, Value);
        }

    }
}
