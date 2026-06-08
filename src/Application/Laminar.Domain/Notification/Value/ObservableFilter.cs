using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notification.Value;

public static class ObservableFilter
{
    extension<T>(IObservable<T> source)
    {
        public IObservable<T> Filter(Func<T, T, bool> predicate)
        {
            return new AnonymousObservable<T>(observer =>
            {
                var gate = new object();

                var hasOld = false;
                var oldValue = default(T)!;

                var isDisposed = 0;

                var subscription = source.Subscribe(
                    new AnonymousObserver<T>(
                        onNext: value =>
                        {
                            if (Volatile.Read(ref isDisposed) == 1)
                                return;

                            lock (gate)
                            {
                                if (!hasOld)
                                {
                                    hasOld = true;
                                    oldValue = value;

                                    observer.OnNext(value);
                                    return;
                                }

                                if (predicate(oldValue, value))
                                {
                                    oldValue = value;
                                    observer.OnNext(value);
                                }
                            }
                        },
                        onError: observer.OnError,
                        onCompleted: observer.OnCompleted
                    )
                );

                return new DisposableAction(() =>
                {
                    if (Interlocked.Exchange(ref isDisposed, 1) == 1)
                        return;

                    subscription.Dispose();
                });
            });
        }

        public IObservable<T> Filter(Func<T, bool> predicate)
        {
            return new AnonymousObservable<T>(observer =>
            {
                var subscription = source.Subscribe(
                    new AnonymousObserver<T>(
                        onNext: value =>
                        {
                            if (predicate(value))
                            {
                                observer.OnNext(value);
                            }
                        },
                        onError: observer.OnError,
                        onCompleted: observer.OnCompleted
                    )
                );

                return subscription;
            });
        }
    }
}