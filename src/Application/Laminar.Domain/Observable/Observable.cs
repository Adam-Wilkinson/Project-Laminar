namespace Laminar.Domain.Observable;

public class Observable<T> : IObservable<T>
{
    private readonly List<IObserver<T>> observers = new();

    public T? LastValue { get; private set; }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        observers.Add(observer);
        return new Disposer(observer, observers);
    }

    public void ChangeValue(T newValue)
    {
        foreach (IObserver<T> observer in observers)
        {
            observer.OnNext(newValue);
        }

        LastValue = newValue;
    }

    private class Disposer : IDisposable
    {
        private readonly IObserver<T> observer;
        private readonly List<IObserver<T>> observerList;

        public Disposer(IObserver<T> observer, List<IObserver<T>> observerList)
        {
            this.observer = observer;
            this.observerList = observerList;
        }

        public void Dispose()
        {
            observerList.Remove(observer);
        }
    }
}
