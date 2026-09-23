using System.Collections;

namespace Laminar.Domain.Observables.Collections;

public class ToWritable<T>(IReadOnlyCollection<T> readOnlyCollection) : ICollection<T>
{
    public IEnumerator<T> GetEnumerator() => readOnlyCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(T item) => throw new InvalidOperationException("This collection is read-only");

    public void Clear() => throw new InvalidOperationException("This collection is read-only");

    public bool Contains(T item) => readOnlyCollection.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => readOnlyCollection.ToArray().CopyTo(array, arrayIndex);

    public bool Remove(T item) => throw new InvalidOperationException("This collection is read-only");

    public int Count => readOnlyCollection.Count;

    public bool IsReadOnly => true;
}

public static class ToWritableExtension
{
    public static ICollection<T> ToWritable<T>(this IReadOnlyCollection<T> readOnlyCollection) 
        => new ToWritable<T>(readOnlyCollection);
}