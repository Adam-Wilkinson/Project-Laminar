namespace Laminar.Domain;

public interface IReadOnlyObservableCollection<T> : IEnumerable<T>
{
    public T this[Index index] { get; }

    public event EventHandler<T> ItemAdded;

    public event EventHandler<T> ItemRemoved;
}
