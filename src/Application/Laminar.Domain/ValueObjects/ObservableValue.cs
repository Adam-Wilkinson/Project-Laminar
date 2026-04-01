using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

/// <summary>
/// A lightweight implementation of <see cref="INotifyPropertyChanged"/>
/// </summary>
/// <typeparam name="T">The type of the child value</typeparam>
public class ObservableValue<T> : IObservableValue<T>, IValueSink<T>
{
    public ObservableValue(T value) : this()
    {
        Value = value;
    }

    public ObservableValue()
    {
    }

    public T Value
    {
        get;
        set
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            var oldValue = field;
            field = value;
            OnValueChanged?.Invoke(this, EventArgs.Empty);
            ValueChanged?.Invoke(this, new ObservableValueChangedEventArgs<T>(oldValue, field));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    } = default!;

    public event EventHandler? OnValueChanged;

    public event EventHandler<ObservableValueChangedEventArgs<T>>? ValueChanged;
    
    public event PropertyChangedEventHandler? PropertyChanged;
}