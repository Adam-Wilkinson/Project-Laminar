using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

/// <summary>
/// A lightweight implementation of <see cref="INotifyPropertyChanged"/>
/// </summary>
/// <typeparam name="T">The type of the child value</typeparam>
public class ObservableValue<T> : INotifyPropertyChanged
{
    private T _value;

    public ObservableValue(T initialValue)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
