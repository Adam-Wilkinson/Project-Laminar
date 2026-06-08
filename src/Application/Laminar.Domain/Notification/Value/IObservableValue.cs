using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notification.Value;

public interface IObservableValue<T> : IReadOnlyObservableValue<T>, IValueSink<T>
{   
    public new T Value { get; set; }
}