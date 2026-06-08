using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notification.Value;

public static class CombineLatestExtension
{
    public static IObservable<TResult> CombineLatest<T1, T2, TResult>(
        this IObservable<T1> first,
        IObservable<T2> second,
        Func<T1, T2, TResult> selector)
    {
        return new AnonymousObservable<TResult>(observer =>
        {
            var gate = new Lock();

            var hasFirst = false;
            var hasSecond = false;

            T1 latestFirst = default!;
            T2 latestSecond = default!;

            var firstCompleted = false;
            var secondCompleted = false;

            var sub1 = first.Subscribe(
                value =>
                {
                    lock (gate)
                    {
                        latestFirst = value;
                        hasFirst = true;

                        if (hasSecond)
                        {
                            observer.OnNext(
                                selector(latestFirst, latestSecond));
                        }
                    }
                },
                observer.OnError,
                () =>
                {
                    lock (gate)
                    {
                        firstCompleted = true;

                        if (secondCompleted)
                        {
                            observer.OnCompleted();
                        }
                    }
                });

            var sub2 = second.Subscribe(
                value =>
                {
                    lock (gate)
                    {
                        latestSecond = value;
                        hasSecond = true;

                        if (hasFirst)
                        {
                            observer.OnNext(
                                selector(latestFirst, latestSecond));
                        }
                    }
                },
                observer.OnError,
                () =>
                {
                    lock (gate)
                    {
                        secondCompleted = true;

                        if (firstCompleted)
                        {
                            observer.OnCompleted();
                        }
                    }
                });

            return new CompositeDisposable(sub1, sub2);
        });
    }
}