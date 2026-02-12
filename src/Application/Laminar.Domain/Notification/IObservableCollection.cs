using System.Collections;
using System.Runtime.CompilerServices;

namespace Laminar.Domain.Notification;

public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, IList<T>, IList
{
    public new T this[int index] { get; set; }

    public new bool IsReadOnly { get; }

    public new int Count { get; }
    
    public new bool Contains(T value);

    public new void Clear();
    
    public new int IndexOf(T value);

    public new void RemoveAt(int index);
    
    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = ForceCast(value);
    }

    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = ForceCast(value);
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    int IList.Add(object? value)
    {
        Add(ForceCast(value));
        return ((IList<T>)this).Count - 1;
    }

    bool IList.Contains(object? value) => value is T typedValue && Contains(typedValue);
    
    bool ICollection<T>.Contains(T item) => Contains(item);
    
    bool IReadOnlyObservableCollection<T>.Contains(T value) => Contains(value);
    
    void IList.Clear() => Clear();
    
    void ICollection<T>.Clear() => Clear();

    bool IList.IsReadOnly => IsReadOnly;

    bool ICollection<T>.IsReadOnly => IsReadOnly;

    bool IList.IsFixedSize => false;

    int IList.IndexOf(object? value) => value is T typedValue ? IndexOf(typedValue) : -1;
    
    int IReadOnlyObservableCollection<T>.IndexOf(T value) => IndexOf(value);
    
    int IList<T>.IndexOf(T value) => IndexOf(value);

    void IList.Insert(int index, object? value) => Insert(index, ForceCast(value));

    void IList.Remove(object? value) => Remove(ForceCast(value));

    void IList.RemoveAt(int index) => RemoveAt(index);

    void IList<T>.RemoveAt(int index) => RemoveAt(index);

    void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);

    int ICollection.Count => Count;

    int ICollection<T>.Count => Count;

    object ICollection.SyncRoot => throw new InvalidOperationException("List synchronisation not supported");

    bool ICollection.IsSynchronized => false;
    
    private static T ForceCast(object? inputObject, [CallerArgumentExpression("inputObject")] string inputObjectRaw = "input") 
        => inputObject is T typedInput ? typedInput : throw new InvalidTypeException(inputObjectRaw);
    
    private class InvalidTypeException(string parameterName)
        : Exception($"Parameter {parameterName} must be of type {typeof(T)}");
}