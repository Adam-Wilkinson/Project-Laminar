namespace Laminar.Domain.Observables.Value;

public readonly struct ObservableValueChangedEventArgs<T>(T oldValue, T newValue)
{
    public T OldValue => oldValue;
    
    public T NewValue => newValue;
}