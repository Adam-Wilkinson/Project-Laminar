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
using Laminar_Core.Scripts;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class NodeContainer<T> 
        : INodeContainer where T : INode
    {
        private T _baseNode;
        private readonly IObjectFactory _factory;

        public NodeContainer(NodeDependencyAggregate dependencies)
        {
            (Location, ErrorState, NameLabel, FieldList, _factory) = dependencies;
            Name = _factory.GetImplementation<IVisualNodeComponentContainer>();
            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(FieldList.VisualNodeComponentsObservable, _factory);
        }

        public virtual T BaseNode
        {
            get
            {
                _baseNode ??= _factory.CreateInstance<T>();

                return _baseNode;
            }
            set
            {
                _baseNode = value;
                NameLabel.LabelText.Value = _baseNode.NodeName;
                NameLabel.ParentNode = _baseNode;
                Name.Child = NameLabel;

                INodeContainer.NodeBases.Remove(_baseNode);
                INodeContainer.NodeBases.Add(_baseNode, this);

                FieldList.ParentNode = _baseNode;
                FieldList.RemoveAll();
                FieldList.AddRange(BaseNode.Fields);
            }
        }

        protected INodeComponentList FieldList { get; }

        public IVisualNodeComponentContainer Name { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IPoint Location { get; }

        public INotifyCollectionChanged Fields { get; }

        public IVisualNodeComponentContainer FlowOutContainer { get; protected set; }

        public IEditableNodeLabel NameLabel { get; }

        public virtual INodeContainer DuplicateNode() => new NodeFactory(_factory).Get<T>();

        public virtual void Update(IAdvancedScriptInstance instance)
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