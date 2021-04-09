using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace OpenFlow_Core.Nodes.NodeComponents.Collections
{
    public class VisualComponentsOfCollection : INotifyCollectionChanged, IList, IList<IVisualNodeComponent>
    {
        private readonly ObservableCollection<INodeComponent> _components;

        public VisualComponentsOfCollection(ObservableCollection<INodeComponent> nodeComponents)
        {
            nodeComponents.CollectionChanged += Components_CollectionChanged;
            _components = nodeComponents;
            ComponentsAdded(nodeComponents);
        }

        public int Count { get; private set; } = 0;

        public bool IsFixedSize => false;

        public bool IsReadOnly => true;

        public bool IsSynchronized => false;

        public object SyncRoot => false;

        IVisualNodeComponent IList<IVisualNodeComponent>.this[int index] { get => this[index] as IVisualNodeComponent; set => this[index] = value; }

        public object this[int index] 
        { 
            get
            {
                Debug.WriteLine("Slow indexer called, don't like, bad");
                if (index >= Count || index < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                IEnumerator<IVisualNodeComponent> enumer = GetEnumerator();
                for (int i = 0; i < index; i++)
                {
                    enumer.MoveNext();
                }

                return enumer.Current;
            }
            set => throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<IVisualNodeComponent> GetEnumerator()
        {
            foreach (INodeComponent component in _components)
            {
                if (!component.IsVisible)
                {
                    continue;
                }

                foreach (IVisualNodeComponent field in component.VisualComponentList)
                {
                    yield return field;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(object value)
        {
            if (value is not IVisualNodeComponent)
            {
                throw new InvalidCastException($"{nameof(VisualComponentsOfCollection)} only stores valus of type {nameof(IVisualNodeComponent)}");
            }
            return Contains(value as IVisualNodeComponent);
        }

        public bool Contains(IVisualNodeComponent item) => IndexOf(item) != -1;

        public int IndexOf(object value) => GetIndexOf(value);

        public int IndexOf(IVisualNodeComponent item) => GetIndexOf(item);

        public void CopyTo(Array array, int index) => CopyTo(array as IVisualNodeComponent[], index);

        public void CopyTo(IVisualNodeComponent[] array, int arrayIndex)
        {
            IEnumerator<IVisualNodeComponent> enumer = GetEnumerator();
            int i = 0;
            while (enumer.MoveNext())
            {
                array[i + arrayIndex] = enumer.Current;
                i++;
            }
        }

        public bool Remove(IVisualNodeComponent item) => throw new NotSupportedException();

        public void Insert(int index, object value) => throw new NotSupportedException();

        public void Remove(object value) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        public int Add(object value) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public void Insert(int index, IVisualNodeComponent item) => throw new NotSupportedException();

        public void Add(IVisualNodeComponent item) => throw new NotSupportedException();

        private int GetLinearIndexFromNested(int nested)
        {
            int output = 0;
            for (int i = 0; i < nested; i++)
            {
                output += _components[i].VisualComponentList.Count;
            }
            return output;
        }

        private void Components_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ComponentsAdded(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ComponentsRemoved(e.OldItems, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ComponentsRemoved(e.OldItems, e.OldStartingIndex);
                    ComponentsAdded(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Count = 0;
                    CollectionChanged?.Invoke(this, e);
                    break;
                case NotifyCollectionChangedAction.Move:
                    ComponentsRemoved(e.OldItems, e.OldStartingIndex);
                    ComponentsAdded(e.NewItems, e.NewStartingIndex);
                    break;
            }
        }

        private void ComponentsRemoved(IList components, int index)
        {
            foreach (INodeComponent component in components)
            {
                if (component is INodeComponentCollection componentCollection)
                {
                    componentCollection.VisualNodeComponentsObservable.CollectionChanged -= Child_NodeFieldsList_Changed;
                }

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, component.VisualComponentList, GetLinearIndexFromNested(index)));
                Count -= component.VisualComponentList.Count;
                component.VisibilityChanged -= NodePart_VisibilityChanged;
            }
        }

        private void ComponentsAdded(IList components, int index = -1)
        {
            foreach (INodeComponent component in components)
            {
                if (component is INodeComponentCollection componentCollection)
                {
                    componentCollection.VisualNodeComponentsObservable.CollectionChanged += Child_NodeFieldsList_Changed;
                }

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, component.VisualComponentList, GetLinearIndexFromNested(index)));
                Count += component.VisualComponentList.Count;
                component.VisibilityChanged += NodePart_VisibilityChanged;
            }
        }

        private void NodePart_VisibilityChanged(object sender, bool e)
        {
            if (sender is INodeComponent component)
            {
                if (e)
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, component.VisualComponentList, GetLinearIndexFromNested(_components.IndexOf(component))));
                    Count += component.VisualComponentList.Count;
                }
                else
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, component.VisualComponentList, GetLinearIndexFromNested(_components.IndexOf(component))));
                    Count -= component.VisualComponentList.Count;
                }
            }
        }

        private void Child_NodeFieldsList_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Count += e.NewItems.Count;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Count -= e.OldItems.Count;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Count = Count - e.OldItems.Count + e.NewItems.Count;
                    break;
            }

            int i = 0;
            int startingIndex = 0;
            while (i < _components.Count && _components[i].VisualComponentList != sender)
            {
                startingIndex += _components[i].VisualComponentList.Count;
                i++;
            }

            CollectionChanged?.Invoke(this, ChangeCollectionChangedIndex(e, startingIndex, (sender as IList).Count));
        }

        private static NotifyCollectionChangedEventArgs ChangeCollectionChangedIndex(NotifyCollectionChangedEventArgs e, int offset, int listLength) 
        {
            int newStartingIndex = (e.NewStartingIndex == -1 ? listLength - 1 : e.NewStartingIndex) + offset;
            int oldStartingIndex = (e.OldStartingIndex == -1 ? listLength - 1 : e.OldStartingIndex) + offset;
            return (e.Action) switch
            {
                NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, newStartingIndex),
                NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, oldStartingIndex),
                NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, newStartingIndex),
                NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, newStartingIndex, oldStartingIndex),
                _ => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            };
        }

        private int GetIndexOf(object value)
        {
            IEnumerator<IVisualNodeComponent> enumer = GetEnumerator();
            for (int i = 0; i < Count; i++)
            {
                enumer.MoveNext();
                if (enumer.Current == value)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
