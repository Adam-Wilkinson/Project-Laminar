using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

/// <summary>
/// A lightweight implementation of <see cref="INotifyPropertyChanged"/>
/// </summary>
/// <typeparam name="T">The type of the child value</typeparam>
public class ObservableValue<T> : IObservableValue<T>, IValueSink<T>
{
    public ObservableValue(T value)
    {
        Value = value;
    }

    public T Value
    {
        get;
        set
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            T oldValue = field;
            field = value;
            PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
            OnChanged?.Invoke(this, new ObservableValueChangedEventArgs<T>(oldValue, field));
            CovariantOnChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CovariantOnChanged;

    public event EventHandler<ObservableValueChangedEventArgs<T>>? OnChanged;
    
    public event PropertyChangedEventHandler? PropertyChanged;
}