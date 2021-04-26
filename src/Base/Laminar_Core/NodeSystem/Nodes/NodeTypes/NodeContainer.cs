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
using System.Collections;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class NodeContainer<T> 
        : INodeContainer where T : INode, new()
    {
        private T _baseNode;
        private bool _isUpdating = false;
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

        public virtual bool IsLive { get; set; }

        protected INodeComponentList FieldList { get; }

        public IVisualNodeComponentContainer Name { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IPoint Location { get; }

        public INotifyCollectionChanged Fields { get; }

        public IEditableNodeLabel NameLabel { get; }

        public bool HasFields => FieldList.VisualComponentList.Count > 0;

        public virtual INodeContainer DuplicateNode() => new NodeFactory(_factory).Get<T>();

        public void Update(IAdvancedScriptInstance instance)
        {
            if (_isUpdating)
            {
                return;
            }

            _isUpdating = true;

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.InputConnector.Activate(instance, Connection.PropagationDirection.Backwards);
            }

            SafeUpdate(instance);

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.OutputConnector.Activate(instance, Connection.PropagationDirection.Forwards);
            }

            NameLabel.FlowOutput.Activate();

            _isUpdating = false;
        }

        public virtual void SetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue, object value)
        {
            laminarValue.Value = value;
        }

        public virtual object GetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue)
        {
            return laminarValue.Value;
        }

        protected virtual void SafeUpdate(IAdvancedScriptInstance instance)
        {

        }

        protected IVisualNodeComponentContainer GetContainer(IVisualNodeComponent component)
        {
            if (component == NameLabel)
            {
                return Name;
            }

            int index = FieldList.VisualComponentList.IndexOf(component);
            if (index is -1)
            {
                return null;
            }

            return ((IList<IVisualNodeComponentContainer>)Fields)[index];
        }
    }
}