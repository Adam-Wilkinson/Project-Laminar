using OpenFlow_PluginFramework.Primitives;
using System.ComponentModel;

namespace OpenFlow_Core.Primitives
{
    public class ObservableValue<T> : IObservableValue<T>
    {
        private T _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (!value.Equals(_value))
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }
    }
}
