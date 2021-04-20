using System;
using System.Collections.Specialized;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework;
using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Primitives.ObservableCollectionMapper;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Primitives;
using System.Collections.Generic;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class NodeBase<T> : INodeBase where T : INode
    {
        private readonly IObjectFactory _factory;

        public NodeBase(NodeDependencyAggregate dependencies)
        {
            BaseNode = (T)Activator.CreateInstance(typeof(T));
            (Location, ErrorState, NameLabel, _factory) = dependencies;
            NameLabel.LabelText.Value = BaseNode.NodeName;
            NameLabel.ParentNode = BaseNode;
            Name = _factory.GetImplementation<IVisualNodeComponentContainer>();
            Name.Child = NameLabel;

            INodeBase.NodeBases.Add(BaseNode, this);

            FieldList = Constructor.NodeComponentList(BaseNode.Fields);
            FieldList.ParentNode = BaseNode;
            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(FieldList.VisualNodeComponentsObservable, _factory);
        }

        protected T BaseNode { get; }

        protected INodeComponentList FieldList { get; }

        public IVisualNodeComponentContainer Name { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IPoint Location { get; }

        public INotifyCollectionChanged Fields { get; }

        public IVisualNodeComponentContainer FlowOutContainer { get; protected set; }

        public IEditableNodeLabel NameLabel { get; }

        public virtual INodeBase DuplicateNode() => new NodeFactory(_factory).Get<T>();

        public virtual void Update()
        {
        }

        public virtual void MakeLive()
        {
        }

        protected IVisualNodeComponentContainer GetContainer(IVisualNodeComponent component)
        {
            if (component == NameLabel)
            {
                return Name;
            }

            return ((IList<IVisualNodeComponentContainer>)Fields)[FieldList.VisualComponentList.IndexOf(component)];
        }
    }
}