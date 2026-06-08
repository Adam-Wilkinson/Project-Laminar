namespace Laminar.Domain.Notification.Value;

public sealed class AnonymousObserver<T>(
    Action<T>? onNext = null,
    Action<Exception>? onError = null,
    Action? onCompleted = null)
    : IObserver<T>
{
    private readonly Action<T> _onNext = onNext ?? (_ => { });
    private readonly Action<Exception> _onError = onError ?? (_ => { });
    private readonly Action _onCompleted = onCompleted ?? (() => { });

    public void OnNext(T value) => _onNext(value);

    public void OnError(Exception error) => _onError(error);

    public void OnCompleted() => _onCompleted();
}