namespace Laminar.Domain.Notification.Value;

public interface IReadOnlyObservableValue<T> : ICovariantObservableValue<T>
{
    public event EventHandler<ObservableValueChangedEventArgs<T>>? OnChanged;
}