namespace OpenFlow_Core.Nodes
{
    using System;
    using System.Collections.Specialized;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;
    using OpenFlow_PluginFramework;
    using OpenFlow_Core.Nodes.NodeComponents.Visuals;
    using OpenFlow_Core.Primitives.ObservableCollectionMapper;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

    public class NodeBase : INodeBase
    {
        private readonly INode _baseNode;
        private readonly INodeComponentCollection _fieldSection;
        private bool _evaluating;

        public NodeBase(INode baseNode, IObservableValue<bool> errorState)
        {
            INodeBase.NodeBases.Add(baseNode, this);
            ErrorState = errorState;
            _baseNode = baseNode;
            _fieldSection = Constructor.NodeComponentList(baseNode.Fields);
            _fieldSection.ParentNode = baseNode;

            Fields = ObservableCollectionMapper<IVisualNodeComponent, IVisualNodeComponentContainer>.Create(_fieldSection.VisualNodeComponentsObservable);

            TryEvaluate();
        }

        public IObservableValue<bool> ErrorState { get; }

        public double X { get; set; }

        public double Y { get; set; }

        public object Tag { get; set; }

        public string Name => _baseNode.NodeName;

        public INotifyCollectionChanged Fields { get; }

        public INodeBase DuplicateNode() => new NodeBase((INode)Activator.CreateInstance(_baseNode.GetType()), ErrorState.Clone());

        public void TryEvaluate()
        {
            if (!_evaluating)
            {
                _evaluating = true;
                try
                {
                    _baseNode.Evaluate();
                    ErrorState.Value = false;
                }
                catch
                {
                    ErrorState.Value = true;
                }

                _evaluating = false;
            }
        }

        public void DeepUpdate()
        {
            foreach (IVisualNodeComponent component in _fieldSection.VisualComponentList)
            {
                // if (component.ConnectionManager.InputConnector is ValueConnector valInput && valInput.ExclusiveConnection != null)
                // {
                //    valInput.ExclusiveConnection.ParentNode.DeepUpdate();
                // }
            }

            TryEvaluate();
        }
    }
}