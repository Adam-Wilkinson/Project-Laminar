namespace OpenFlow_Core.Nodes.NodeComponents.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;

    /// <summary>
    /// Defines a read only collection of NodeComponents, with protected list methods for children to make public
    /// </summary>
    public class NodeComponentCollection : NodeComponent, INodeComponentCollection
    {
        private INode _parentNode;
        private readonly ObservableCollection<INodeComponent> _childComponents = new();

        public NodeComponentCollection(IOpacity opacity) : base(opacity) 
        {
            VisualComponentList = new NodeFieldList(_childComponents);
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

        public override NodeFieldList VisualComponentList { get; }

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

        public override INodeComponent Clone() => CloneTo(Constructor.NodeComponentList(_childComponents.Select(x => x.Clone())));

        protected virtual void ProtectedAdd(INodeComponent newComponent)
        {
            ProtectedInsert(_childComponents.Count, newComponent);
        }

        protected virtual void ProtectedInsert(int index, INodeComponent newComponent)
        {
            _childComponents.Insert(index, newComponent);
            newComponent.Opacity.AddOpacityFactor(Opacity);
            newComponent.ParentNode = ParentNode;
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
