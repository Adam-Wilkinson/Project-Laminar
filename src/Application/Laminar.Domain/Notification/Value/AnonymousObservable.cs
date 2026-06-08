namespace Laminar.Domain.Notification.Value;

public sealed class AnonymousObservable<T>(Func<IObserver<T>, IDisposable> subscribe) : IObservable<T>
{
    public IDisposable Subscribe(IObserver<T> observer)
        => subscribe(observer);
}