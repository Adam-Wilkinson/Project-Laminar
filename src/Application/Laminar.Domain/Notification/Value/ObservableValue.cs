using System.ComponentModel;

namespace Laminar.Domain.Notification.Value;

public class ObservableValue<T>(T value) : ObservableValueBase<T>
{
    public override T Value { get; set => SetAndRaise(ref field, value); } = value;
}

public abstract class ObservableValueBase<T> : IObservableValue<T>
{
    private readonly HashSet<Subscription> _subscriptions = [];
    
    public abstract T Value { get; set; }

    public event EventHandler<ObservableValueChangedEventArgs<T>>? OnChanged;
    public event EventHandler? CovariantOnChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetAndRaise(ref T currentValue, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
        T oldValue = currentValue;
        
        BeforeValueChanged();
        
        currentValue = newValue;
        foreach (var subscription in _subscriptions)
        {
            subscription.OnNext(newValue);
        }
        
        AfterValueChanged();
        
        OnChanged?.Invoke(this, new ObservableValueChangedEventArgs<T>(oldValue, newValue));
        CovariantOnChanged?.Invoke(this, EventArgs.Empty);
        PropertyChanged?.Invoke(this, IObservableValueBase.ValueChangedEventArgs);
        return true;
    }

    protected virtual void BeforeValueChanged()
    {
        
    }
    
    /// <summary>
    /// Called by SetAndRaise after the value has been updated, but before any events are fired
    /// </summary>
    protected virtual void AfterValueChanged()
    {
        
    }

    public IDisposable Subscribe(IObserver<T> observer) => new Subscription(this, observer);

    private class Subscription : IDisposable
    {
        private readonly ObservableValueBase<T> _parent;
        private readonly IObserver<T> _target;

        public Subscription(ObservableValueBase<T> parent, IObserver<T> target)
        {
            _parent = parent;
            _target = target;
            _parent._subscriptions.Add(this);
            _target.OnNext(_parent.Value);
        }

        public void OnNext(T value)
        {
            _target.OnNext(value);
        }
        
        public void Dispose()
        {
            _target.OnCompleted();
            _parent._subscriptions.Remove(this);
        }
    }
}