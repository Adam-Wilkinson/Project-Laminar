using System;
using System.Collections.Specialized;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework;
using OpenFlow_Core.NodeSystem.NodeComponents.Visuals;
using OpenFlow_Core.Primitives.ObservableCollectionMapper;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System.Collections;
using OpenFlow_Core.Primitives;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class NodeBase : INodeBase
    {
        private readonly INode _baseNode;
        private readonly INodeComponentCollection _fieldSection;
        private bool _updating;

        public NodeBase(INode baseNode, NodeDependencyAggregate dependencies)
        {
            INodeBase.NodeBases.Add(baseNode, this);

            ErrorState = dependencies.ErrorState;
            Location = dependencies.Location;
            Name = dependencies.Name;

            _baseNode = baseNode;
            _fieldSection = Constructor.NodeComponentList(baseNode.Fields);
            _fieldSection.ParentNode = baseNode;
            Name.Value = _baseNode.NodeName;

            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(_fieldSection.VisualNodeComponentsObservable);

            Update();
        }

        public IObservableValue<bool> ErrorState { get; }

        public IPoint Location { get; }

        public IObservableValue<string> Name { get; }

        public INotifyCollectionChanged Fields { get; }

        public INodeBase DuplicateNode() => new NodeBase((INode)Activator.CreateInstance(_baseNode.GetType()), Instance.Factory.CreateInstance<NodeDependencyAggregate>());

        public void Update()
        {
            if (_updating)
            {
                return;
            }

            _updating = true;

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.InputConnector.Activate();
            }

            try
            {
                if (_baseNode is IFunctionNode functionNode)
                {
                    functionNode.Evaluate();
                }
                ErrorState.Value = false;
            }
            catch
            {
                ErrorState.Value = true;
            }

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.OutputConnector.Activate();
            }

            _updating = false;
        }
    }
}