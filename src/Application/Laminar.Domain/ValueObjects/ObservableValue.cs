using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

public class ObservableValue<T>(T value) : ObservableValueBase<T>
{
    public override T Value { get; set => SetAndRaise(ref field, value); } = value;
}

public abstract class ObservableValueBase<T> : IObservableValue<T>
{
    public abstract T Value { get; set; }

    public event EventHandler<ObservableValueChangedEventArgs<T>>? OnChanged;
    public event EventHandler? CovariantOnChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetAndRaise(ref T currentValue, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
        T oldValue = currentValue;
        
        BeforeValueChanged();
        currentValue = newValue;
        AfterValueChanged();
        
        OnChanged?.Invoke(this, new ObservableValueChangedEventArgs<T>(oldValue, newValue));
        CovariantOnChanged?.Invoke(this, EventArgs.Empty);
        PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
        return true;
    }

    protected virtual void BeforeValueChanged()
    {
        
    }
    
    /// <summary>
    /// Called by SetAndRaise after the value has been updated, but before any events are fired
    /// </summary>
    protected virtual void AfterValueChanged()
    {
        
    }
}