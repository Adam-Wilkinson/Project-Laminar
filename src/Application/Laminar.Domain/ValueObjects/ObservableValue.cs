using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

/// <summary>
/// A lightweight implementation of <see cref="INotifyPropertyChanged"/>
/// </summary>
/// <typeparam name="T">The type of the child value</typeparam>
public class ObservableValue<T> : IObservableValue<T>, IValueSink<T>
{
    private T _value = default!;

    public ObservableValue(T value) : this()
    {
        Value = value;
    }

    public ObservableValue()
    {
    }
    
    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;
            
            _value = value;
            ValueChanged?.Invoke(this, _value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    public event EventHandler<T>? ValueChanged;
    
    public event PropertyChangedEventHandler? PropertyChanged;
}
