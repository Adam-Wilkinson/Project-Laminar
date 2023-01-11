using System.Collections;

namespace Laminar.Domain.Notification;

public class ObservableCollection<T> : IObservableCollection<T>
{
    private List<T> _internalList = new();

    public T this[Index index] => _internalList[index];

    public event EventHandler<T>? ItemAdded;

    public event EventHandler<T>? ItemRemoved;

    public void Add(T item)
    {
        _internalList.Add(item);
        OnAdd(item);
        ItemAdded?.Invoke(this, item);
    }

    public IEnumerator<T> GetEnumerator() => _internalList.GetEnumerator();

    public bool Remove(T item)
    {
        if (_internalList.Remove(item))
        {
            OnRemove(item);
            ItemRemoved?.Invoke(this, item);
            return true;
        }

        return false;
    }

    protected virtual void OnAdd(T newItem) { }

    protected virtual void OnRemove(T newItem) { }

    IEnumerator IEnumerable.GetEnumerator() => _internalList.GetEnumerator();
}
