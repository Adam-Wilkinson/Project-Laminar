namespace Laminar.Domain.Notification.Value;

public static class ObservableExtensions
{
    extension<T>(IObservable<T> source)
    {
        public IDisposable Subscribe(Action<T> onNext) => source.Subscribe(new AnonymousObserver<T>(onNext: onNext));

        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError) =>
            source.Subscribe(new AnonymousObserver<T>(onNext: onNext, onError: onError));

        public IDisposable Subscribe(Action<T> onNext, Action<Exception> onError, Action onCompleted) =>
            source.Subscribe(new AnonymousObserver<T>(onNext: onNext, onError: onError, onCompleted: onCompleted));
    }
}