using System.Collections.Specialized;
using Laminar.Domain.Extensions;

namespace Laminar.Domain.Observables.Collections;

public static class ObservableBagToListExtension
{
    public static IReadOnlyObservableList<T> ToObservableList<T>(this IReadOnlyObservableBag<T> bag, IComparer<T> comparer)
        where T : notnull
        => new BagToList<T>(bag, comparer);

    private class BagToList<T> : ReadOnlyObservableListBase<T> where T : notnull
    {
        private readonly IComparer<T> _comparer;
        private readonly List<T> _list;
        
        public BagToList(IReadOnlyObservableBag<T> bag, IComparer<T> comparer)
        {
            _comparer = comparer;
            _list = [.. bag];
            _list.Sort(_comparer);
            bag.SubscribeForEach(OnItemAdded, OnItemRemoved);
        }

        private void OnItemRemoved(T obj)
        {
            var index = _list.BinarySearch(obj, _comparer);
            if (index < 0) return;
            
            _list.RemoveAt(index);
            InvokeCollectionChanged(this, NotifyCollectionChangedEventArgs.Remove(obj, index));
        }

        private void OnItemAdded(T obj)
        {
            var index = _list.BinarySearch(obj, _comparer);
            index = index >= 0 ? index : ~index;
            _list.Insert(index, obj);
            InvokeCollectionChanged(this, NotifyCollectionChangedEventArgs.Insert(obj, index));
        }

        public override bool Contains(T value) => _list.BinarySearch(value, _comparer) > 0;

        public override int IndexOf(T value)
        {
            var index = _list.BinarySearch(value, _comparer);
            return index >= 0 ? index : -1;
        }

        public override IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public override int Count => _list.Count;

        public override T this[int index] => _list[index];
    }
}