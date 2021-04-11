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

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class NodeBase : INodeBase
    {
        private readonly INode _baseNode;
        private readonly INodeComponentCollection _fieldSection;
        private bool _updating;

        public NodeBase(INode baseNode, IObservableValue<bool> errorState)
        {
            INodeBase.NodeBases.Add(baseNode, this);
            ErrorState = errorState;
            _baseNode = baseNode;
            _fieldSection = Constructor.NodeComponentList(baseNode.Fields);
            _fieldSection.ParentNode = baseNode;

            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(_fieldSection.VisualNodeComponentsObservable);

            Update();
        }

        public IObservableValue<bool> ErrorState { get; }

        public double X { get; set; }

        public double Y { get; set; }

        public object Tag { get; set; }

        public string Name => _baseNode.NodeName;

        public INotifyCollectionChanged Fields { get; }

        public INodeBase DuplicateNode() => new NodeBase((INode)Activator.CreateInstance(_baseNode.GetType()), ErrorState.Clone());

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