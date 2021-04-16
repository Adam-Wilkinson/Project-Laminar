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
using System.Collections.Generic;

namespace OpenFlow_Core.NodeSystem.Nodes.NodeTypes
{
    public class NodeBase<T> : INodeBase where T : INode
    {
        public NodeBase(NodeDependencyAggregate dependencies)
        {
            BaseNode = (T)Activator.CreateInstance(typeof(T));
            (Location, ErrorState, Name) = dependencies;
            Name.Value = BaseNode.NodeName;

            INodeBase.NodeBases.Add(BaseNode, this);

            FieldList = Constructor.NodeComponentList(BaseNode.Fields);
            FieldList.ParentNode = BaseNode;
            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(FieldList.VisualNodeComponentsObservable);
        }

        protected T BaseNode { get; }

        protected INodeComponentList FieldList { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IPoint Location { get; }

        public IObservableValue<string> Name { get; }

        public INotifyCollectionChanged Fields { get; }

        public IVisualNodeComponentContainer FlowOutContainer { get; protected set; }

        public virtual INodeBase DuplicateNode() => new NodeFactory().Get<T>();// new NodeBase((INode)Activator.CreateInstance(_baseNode.GetType()), Instance.Factory.CreateInstance<NodeDependencyAggregate>());

        public virtual void Update()
        {
        }

        public virtual void MakeLive()
        {
        }

        protected IVisualNodeComponentContainer GetContainer(IVisualNodeComponent component)
        {
            return ((IList<IVisualNodeComponentContainer>)Fields)[FieldList.VisualComponentList.IndexOf(component)];
        }
    }
}