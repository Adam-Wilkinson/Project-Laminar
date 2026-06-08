using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Laminar.Domain.Notification.Collections;

public enum SourcedCollectionMode
{
    /// <summary>
    /// When a sync occurs, the list is arranged to be equal
    /// </summary>
    SequenceEquality = 0,
    
    /// <summary>
    /// When a sync occurs, the two lists will contain the same items, but not necessarily in the same order
    /// </summary>
    SetEquality = 1,
}

public class SourcedObservableCollection<T> : IObservableCollection<T> where T : notnull
{
    private readonly List<T> _internalList = [];
    private readonly IEqualityComparer<T> _equalityComparer;
    
    private IEnumerable<T> _source = [];
    private bool _matchesSource = true;
    private SourcedCollectionMode _mode = SourcedCollectionMode.SequenceEquality;
    
    public SourcedObservableCollection(
        IEnumerable<T> source, 
        IEqualityComparer<T>? equalityCheck = null,
        SourcedCollectionMode mode = SourcedCollectionMode.SequenceEquality)
    {
        _equalityComparer = equalityCheck ?? EqualityComparer<T>.Default;
        ChangeSourceTo(source, mode);
    }

    public void ChangeSourceTo(IEnumerable<T> source, SourcedCollectionMode mode = SourcedCollectionMode.SequenceEquality)
    {
        _source = source;
        _matchesSource = false;
        _mode = mode;
        
        if (source is INotifyCollectionChanged notifyingSource)
        {
            notifyingSource.CollectionChanged += NotifyingSourceChanged;
        }

        SyncFromSource();
    }

    // We will initialize spans manually
    [SkipLocalsInit]
    public void SyncFromSource()
    {
        IList<T> sourceList = _source as IList<T> ?? [.._source];
        
        // Lives in the old index space, either the index things need moving to, or -1 for removals
        Span<int> targetIndices = stackalloc int[_internalList.Count];
        for (int i = 0; i < targetIndices.Length; i++) targetIndices[i] = -1;
        
        // Stores the indices in the new list at which there are new items
        Span<int> newItemIndices = stackalloc int[sourceList.Count];
        int numberOfNewItems = 0;

        int commonItemCount = 0;

        // FIRST PASS: Loop through the new list to populate newItemIndices and targetIndices. 
        // We also determine here how much calculation is necessary
        bool sourceIsSubsetOfOldList = true;
        bool listsHaveSameOrdering = false;
        int matchCount = 0;
        int lastIndex = -1;
        for (int i = 0; i < sourceList.Count; i++)
        {
            int indexInOldList = GetIndexInOutput(sourceList[i]);

            // New item
            if (indexInOldList == -1)
            {
                newItemIndices[numberOfNewItems++] = i;
                sourceIsSubsetOfOldList = false;
                continue;
            }
            matchCount++;
            
            // Determine whether a move pass will be necessary
            if (indexInOldList <= lastIndex)
            {
                listsHaveSameOrdering = false;
            }
            lastIndex = indexInOldList;
            
            // We've already found an item that has this oldIndex
            if (targetIndices[indexInOldList] != -1)
            {
                throw new DuplicateItemException();
            }
            
            // We subtract the numberOfNewItems because they won't be in the list during the move pass.
            targetIndices[indexInOldList] = i - numberOfNewItems;
            commonItemCount++;
        }
        
        bool listsAreSetEqual = sourceIsSubsetOfOldList && matchCount == _internalList.Count;

        if (listsAreSetEqual && listsHaveSameOrdering)
        {
            _matchesSource = true;
            return;
        }
        
        // At each index i, stores the target position of element i.
        // EXAMPLE: Old list: [a, b, c, d]; New list: [a, d, b, c]; movePassTargetIndices: [0, 2, 3, 1]
        Span<int> movePassTargetIndices = stackalloc int[commonItemCount];
        
        // SECOND PASS: Remove items and populate movePassTargetIndices
        int indexInCommonItems = 0;
        int removedItemCount = 0;
        for (int i = 0; i < targetIndices.Length; i++)
        {
            if (targetIndices[i] == -1)
            {
                RemoveAt(i - removedItemCount);
                removedItemCount++;
                continue;
            }
        
            movePassTargetIndices[indexInCommonItems++] = targetIndices[i];
        }

        // The move pass involves expensive calculations to minimize move operations; best avoid if we can
        if (!listsHaveSameOrdering && _mode == SourcedCollectionMode.SequenceEquality)
        {
            // movePassTargetIndices now needs a longest increasing subsequence (LIS) calculation to find
            // the minimum number of move operations required to sync the lists
            Span<int> lis = stackalloc int[commonItemCount];
            int lisLength = ComputeLis(movePassTargetIndices, ref lis);
        
            // We will order the moves by their target index. This ensures that moves don't affect future moves.
            Span<ValueTuple<int, int>> movesInOrder = stackalloc ValueTuple<int, int>[commonItemCount];
            for (int i = 0; i < commonItemCount; i++)
            {
                movesInOrder[movePassTargetIndices[i]] = ValueTuple.Create(i, movePassTargetIndices[i]);
            }

            int indexInLis = 0;
            int lastLisCurrentIndex = 0;
            int lastLisTargetIndex = 0;
            for (int i = 0; i < commonItemCount; i++)
            {
                (int oldIndex, int targetIndex) = movesInOrder[i];

                if (indexInLis < lisLength && targetIndex == lis[indexInLis])
                {
                    // This item will end up in the right place, but we need to offset
                    // So items will be placed appropriately.
                    lastLisCurrentIndex = oldIndex;
                    lastLisTargetIndex = targetIndex;
                    indexInLis++;
                    continue;
                }
            
                if (oldIndex < lastLisCurrentIndex)
                {
                    lastLisCurrentIndex--;
                }
            
                targetIndex += lastLisCurrentIndex - lastLisTargetIndex;
            
                for (int j = targetIndex + 1; j <= oldIndex; j++)
                {
                    movesInOrder[j] = new ValueTuple<int, int>(movesInOrder[j].Item1 + 1, movesInOrder[j].Item2);
                }   
            
                Move(oldIndex, targetIndex);
            }
        }
        
        // Finally, Add pass:
        if (numberOfNewItems <= 0)
        {
            _matchesSource = true;
            return;
        }
        
        int rangeStart = newItemIndices[0];
        int previous = rangeStart;
        int contiguousCount = 1;

        for (int i = 1; i < numberOfNewItems; i++)
        {
            int current = newItemIndices[i];

            if (current == previous + 1)
            {
                contiguousCount++;
            }
            else
            {
                EmitRange(rangeStart, contiguousCount);

                rangeStart = current;
                contiguousCount = 1;
            }

            previous = current;
        }

        EmitRange(rangeStart, contiguousCount);
        
        _matchesSource = true;
        return;

        void EmitRange(int start, int emitCount)
        {
            if (emitCount == 1)
            {
                Insert(start, sourceList[start]);
            }
            else
            {
                InsertRange(
                    start,
                    sourceList.Skip(start).Take(emitCount));
            }
        }
    }
    
    public void Add(T item)
    {
        _internalList.Add(item);
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
    }

    public void Clear()
    {
        _internalList.Clear();
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T value) => _internalList.Contains(value);

    public int IndexOf(T value) => _internalList.IndexOf(value);

    public void Insert(int index, T item)
    {
        _internalList.Insert(index, item);
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void InsertRange(int index, IEnumerable<T> items)
    {
        IList<T> itemsList = items is IList<T> and IList ? (IList<T>)items : [.. items];
        _internalList.InsertRange(index, itemsList);
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)itemsList, index));
    }

    public void RemoveAt(int index)
    {
        T oldItem = _internalList[index];
        _internalList.RemoveAt(index);
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
    }

    public void RemoveRange(int index, int count)
    {
        T[] oldItems = [.. _internalList.Skip(index).Take(count)];
        _internalList.RemoveRange(index, count);
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index));
    }

    public void Move(int oldIndex, int newIndex)
    {
        T item = _internalList[oldIndex];
        _internalList.RemoveAt(oldIndex);
        _internalList.Insert(newIndex, item);
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }

    public void Replace(int index, T newItem)
    {
        T removedItem = _internalList[index];
        _internalList[index] = newItem;
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,  newItem, removedItem, index));
    }

    public void Replace(T oldItem, T newItem)
    {
        int index = _internalList.IndexOf(oldItem);
        _internalList[index] = newItem;
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
    }
    
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => _internalList[index];
        set
        {
            T oldItem = _internalList[index];
            _internalList[index] = value;
            _matchesSource = false;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
        }
    }

    public IEnumerator<T> GetEnumerator() => _internalList.GetEnumerator();

    public int Count => _internalList.Count;

    public void CopyTo(T[] array, int arrayIndex) => _internalList.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        int removeIndex = _internalList.IndexOf(item);
        if (!_internalList.Remove(item)) return false;
        _matchesSource = false;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, removeIndex));
        return true;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
    
    
    private void NotifyingSourceChanged(object? _, NotifyCollectionChangedEventArgs args)
    {
        if (!_matchesSource)
        {
            SyncFromSource();
            return;
        }

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _internalList.InsertRange(args.NewStartingIndex, args.NewItems!.Cast<T>());
                break;
            case NotifyCollectionChangedAction.Remove:
                _internalList.RemoveRange(args.OldStartingIndex, args.OldItems!.Count);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                _internalList.RemoveRange(args.OldStartingIndex, args.OldItems!.Count);
                _internalList.InsertRange(args.NewStartingIndex, args.NewItems!.Cast<T>());
                break;
            case NotifyCollectionChangedAction.Reset:
                _internalList.Clear();
                break;
        }

        CollectionChanged?.Invoke(this, args);
    }
    
    private int GetIndexInOutput(T sourceItem)
    {
        int currentIndex = 0;
        foreach (T item in _internalList)
        {
            if (_equalityComparer.Equals(sourceItem, item))
            {
                return currentIndex;
            }

            currentIndex++;
        }

        return -1;
    }
    
    // Returns the LIS length, and writes the LIS *values* (not indices) into `lis[0..length)`.
    // Allocation-free on the heap; uses stackalloc for scratch.
    private static int ComputeLis(ReadOnlySpan<int> values, ref Span<int> lis)
    {
        int n = values.Length;
        if (n == 0) return 0;
        if (lis.Length < n) throw new ArgumentException("`lis` span is too small.", nameof(lis));

        // tails[len] = index in `values` of the smallest possible tail value for an increasing subsequence of length len+1
        Span<int> tails = stackalloc int[n];

        // prev[i] = index in `values` of the previous element in the LIS ending at i (or -1)
        Span<int> prev = stackalloc int[n];
        for (int i = 0; i < n; i++) prev[i] = -1;

        int size = 0;

        for (int i = 0; i < n; i++)
        {
            int x = values[i];

            // lower_bound on tails by comparing values[tails[mid]] < x
            int lo = 0, hi = size;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (values[tails[mid]] < x) lo = mid + 1;
                else hi = mid;
            }

            int pos = lo;

            if (pos > 0) prev[i] = tails[pos - 1];
            tails[pos] = i;

            if (pos == size) size++;
        }

        // Reconstruct LIS values into lis[0..size)
        int k = tails[size - 1];
        for (int idx = size - 1; idx >= 0; idx--)
        {
            lis[idx] = values[k];
            k = prev[k];
        }

        return size;
    }
}

public class DuplicateItemException(Exception? innerException = null) 
    : Exception("This method requires a distinct set of items as input, but a duplicate was found", innerException);