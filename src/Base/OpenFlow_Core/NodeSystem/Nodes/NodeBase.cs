using System;
using System.Collections.Specialized;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework;
using OpenFlow_Core.NodeSystem.NodeComponents.Visuals;
using OpenFlow_Core.Primitives.ObservableCollectionMapper;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_Core.Primitives;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class NodeBase<T> : INodeBase where T : INode
    {
        private readonly T _baseNode;

        public NodeBase(T baseNode, NodeDependencyAggregate dependencies)
        {
            _baseNode = baseNode;
            (Location, ErrorState, Name) = dependencies;
            Name.Value = _baseNode.NodeName;

            INodeBase.NodeBases.Add(baseNode, this);

            FieldList = Constructor.NodeComponentList(baseNode.Fields);
            FieldList.ParentNode = baseNode;
            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(FieldList.VisualNodeComponentsObservable);
        }

        protected INodeComponentList FieldList { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IPoint Location { get; }

        public IObservableValue<string> Name { get; }

        public INotifyCollectionChanged Fields { get; }

        public virtual INodeBase DuplicateNode() => new NodeFactory().Get<T>();// new NodeBase((INode)Activator.CreateInstance(_baseNode.GetType()), Instance.Factory.CreateInstance<NodeDependencyAggregate>());

        public virtual void Update()
        {
        }
    }
}