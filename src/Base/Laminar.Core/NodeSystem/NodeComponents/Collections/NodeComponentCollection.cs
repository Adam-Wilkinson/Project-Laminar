namespace Laminar_Core.NodeSystem.NodeComponents.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using Laminar_PluginFramework;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework.Primitives;

    /// <summary>
    /// Defines a read only collection of NodeComponents, with protected list methods for children to make public
    /// </summary>
    public class NodeComponentCollection : NodeComponent, INodeComponentCollection
    {
        private INode _parentNode;
        private readonly ObservableCollection<INodeComponent> _childComponents = new();

        public NodeComponentCollection(IOpacity opacity) : base(opacity) 
        {
            VisualComponentList = new VisualComponentsOfCollection(_childComponents);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                _childComponents.CollectionChanged += value;
            }

            remove
            {
                _childComponents.CollectionChanged -= value;
            }
        }

        public override VisualComponentsOfCollection VisualComponentList { get; }

        public INotifyCollectionChanged VisualNodeComponentsObservable => VisualComponentList;

        public override INode ParentNode
        {
            get => _parentNode;
            set
            {
                _parentNode = value;
                foreach (INodeComponent component in _childComponents)
                {
                    component.ParentNode = _parentNode;
                }
            }
        }

        public int ComponentCount => _childComponents.Count;

        public INodeComponent this[Index index]
        {
            get => _childComponents[index];
            protected set
            {
                if (GetIndex(index) >= _childComponents.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                _childComponents[index] = value;
            }
        }

        public IEnumerator<INodeComponent> GetEnumerator() => _childComponents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(INodeComponent component) => _childComponents.IndexOf(component);

        public bool Contains(INodeComponent component) => _childComponents.Contains(component);

        public override INodeComponentCollection Clone() 
        {
            NodeComponentCollection newCollection = new(Opacity.Clone());
            CloneTo(newCollection);
            return newCollection;
        }

        public override void CloneTo(INodeComponent component)
        {
            if (component is not INodeComponentCollection componentCollection)
            {
                throw new ArgumentException("NodeComponentCollection can only clone to other NodeComponentCollection");
            }

            base.CloneTo(component);
            ProtectedCloneTo(componentCollection);
        }

        public virtual void CopyComponentsFrom(INodeComponentCollection componentCollection)
        {
            ProtectedReset();
            foreach (INodeComponent component in componentCollection)
            {
                ProtectedAdd(component.Clone());
            }
        }

        protected virtual void ProtectedCloneTo(INodeComponentCollection collection)
        {
            collection.CopyComponentsFrom(this);
        }

        protected virtual void ProtectedAdd(INodeComponent newComponent)
        {
            ProtectedInsert(_childComponents.Count, newComponent);
        }

        protected virtual void ProtectedInsert(int index, INodeComponent newComponent)
        {
            newComponent.Opacity.AddOpacityFactor(Opacity);
            newComponent.ParentNode = ParentNode;
            _childComponents.Insert(index, newComponent);
        }

        protected virtual void ProtectedRemoveAt(int index)
        {
            if (GetIndex(index) < _childComponents.Count)
            {
                _childComponents.RemoveAt(index);
            }
        }

        protected virtual bool ProtectedRemove(INodeComponent component)
        {
            component.Opacity.RemoveOpacityFactor(Opacity);
            return _childComponents.Remove(component);
        }

        protected virtual void ProtectedReset()
        {
            for (int i = _childComponents.Count - 1; i >= 0; i--)
            {
                ProtectedRemoveAt(i);
            }
        }

        private int GetIndex(Index index) => index.IsFromEnd ? _childComponents.Count - index.Value : index.Value;
    }
}
