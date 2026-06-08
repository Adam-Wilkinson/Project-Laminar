using System.ComponentModel;

namespace Laminar.Domain.Notification.Value;

public interface ICovariantObservableValue<out T> : IObservableValueBase, IObservable<T>
{
    public T Value { get; }

    public event EventHandler CovariantOnChanged;
}

public interface IObservableValueBase : INotifyPropertyChanged
{
    public static readonly PropertyChangedEventArgs ValueChangedEventArgs = new(nameof(ICovariantObservableValue<>.Value));
}