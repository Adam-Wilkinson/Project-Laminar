using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Laminar.Domain.Notification;

/// <summary>
/// Contains no implementation; only redirects boilerplate and connects up the messy list interfaces
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ObservableCollectionBase<T> : IReadOnlyObservableCollection<T>, IList<T>, IList
{
    #region Abstract list implementation

        public abstract void Add(T item);
    
        public abstract void Clear();
        public abstract bool Contains(T value);

        public abstract void CopyTo(T[] array, int arrayIndex);

        public abstract bool Remove(T item);

        public abstract int IndexOf(T value);
        
        public abstract void Insert(int index, T item);

        public abstract void RemoveAt(int index);

        public abstract IEnumerator<T> GetEnumerator();
        
        public abstract int Count { get; }
        
        public abstract T this[int index] { get; set; }
        
    #endregion
    
    protected void InvokeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(sender, e);
    }
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public bool IsReadOnly => false;
    
    #region Wiring up interfaces to the abstract members

        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value!;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        int IList.Add(object? value)
        {
            Add(ForceCast(value));
            return Count - 1;
        }

        bool IList.Contains(object? value) => value is T typedValue && Contains(typedValue);

        void IList.Clear() => Clear();

        bool IList.IsReadOnly => true;

        bool IList.IsFixedSize => false;

        int IList.IndexOf(object? value) => value is T typedValue ? IndexOf(typedValue) : -1;

        void IList.Insert(int index, object? value) => Insert(index, ForceCast(value));

        void IList.Remove(object? value) => Remove(ForceCast(value));

        void IList.RemoveAt(int index) => RemoveAt(index);

        void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);

        int ICollection.Count => Count;

        object ICollection.SyncRoot => throw new InvalidOperationException("List synchronisation not supported");

        bool ICollection.IsSynchronized => false;

    #endregion

    private static T ForceCast(object? inputObject, [CallerArgumentExpression("inputObject")] string inputObjectRaw = "input") 
        => inputObject is T typedInput ? typedInput : throw new InvalidTypeException(inputObjectRaw);
    
    private class InvalidTypeException(string parameterName)
        : Exception($"Parameter {parameterName} must be of type {typeof(T)}");
}