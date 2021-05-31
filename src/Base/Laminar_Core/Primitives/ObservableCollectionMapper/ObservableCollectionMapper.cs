using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar_Core.Primitives.ObservableCollectionMapper
{
    public class ObservableCollectionMapper<TIn, TOut> : IObservableCollectionMapper<TIn, TOut>
    {
        readonly ITypeMapper<TIn, TOut> _mapper;
        readonly Dictionary<INotifyCollectionChanged, MappedCollection> _mappedCollections = new();

        public ObservableCollectionMapper(ITypeMapper<TIn, TOut> mapper)
        {
            _mapper = mapper;
        }

        public ReadOnlyObservableCollection<TOut> Map(INotifyCollectionChanged inputCollection)
        {
            ObservableCollection<TOut> output = new();
            MappedCollection newCollection = new(output, inputCollection, _mapper);
            _mappedCollections.Add(inputCollection, newCollection);
            return new(output);
        }

        private class MappedCollection
        {
            protected readonly ITypeMapper<TIn, TOut> _mapper;
            private readonly ObservableCollection<TOut> _mapTo;

            public MappedCollection(ObservableCollection<TOut> mapTo, INotifyCollectionChanged mapFrom, ITypeMapper<TIn, TOut> mapper)
            {
                _mapper = mapper;
                _mapTo = mapTo;
                foreach (object item in (mapFrom as IList))
                {
                    if (item is TIn itemOut)
                    {
                        mapTo.Add(mapper.MapType(itemOut));
                    }
                }

                mapFrom.CollectionChanged += Collection_CollectionChanged;
            }

            private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        InsertRange(e.NewStartingIndex, e.NewItems);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        InsertRange(e.NewStartingIndex, e.NewItems);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        RemoveRange(0, _mapTo.Count);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        InsertRange(e.NewStartingIndex, e.NewItems);
                        break;
                }
            }

            private void InsertRange(int index, IList list)
            {
                index = index == -1 ? _mapTo.Count - 1 : index;
                int j = 0;
                foreach (object item in list)
                {
                    if (item is TIn itemIn)
                    {
                        _mapTo.Insert(index + j, _mapper.MapType(itemIn));
                        j++;
                    }
                }
            }

            private void RemoveRange(int index, int count)
            {
                index = index == -1 ? _mapTo.Count - 1 : index;
                for (int i = 0; i < count; i++)
                {
                    _mapTo.RemoveAt(index + count - 1 - i);
                }
            }
        }
    }
}