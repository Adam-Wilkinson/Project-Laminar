namespace OpenFlow_Core.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_Core.Nodes.VisualNodeComponentDisplays;
    using OpenFlow_PluginFramework.Primitives;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Nodes.NodeComponents.Collections;
    using OpenFlow_PluginFramework;
    using OpenFlow_Core.Nodes.NodeComponents.Visuals;

    public class NodeBase
    {
        private readonly INode _baseNode;
        private readonly INodeComponentCollection _fieldSection;
        private bool _errorState;
        private bool _evaluating;

        public NodeBase(INode baseNode)
        {
            _baseNode = baseNode;
            _fieldSection = Constructor.NodeComponentList(baseNode.Fields);
            _fieldSection.ParentNode = baseNode;

            baseNode.SubscribeToEvaluate(TryEvaluate);

            TryEvaluate();
        }

        public event EventHandler<bool> ErrorStateChanged;

        public bool ErrorState
        {
            get => _errorState;
            private set
            {
                if (value != _errorState)
                {
                    _errorState = value;
                    ErrorStateChanged?.Invoke(this, _errorState);
                }
            }
        }

        public double X { get; set; }

        public double Y { get; set; }

        public object Tag { get; set; }

        public string Name => _baseNode.NodeName;

        public INotifyCollectionChanged Fields => _fieldSection.VisualNodeComponentsObservable;

        public NodeBase DuplicateNode() => new((INode)Activator.CreateInstance(_baseNode.GetType()));

        /*
        public bool TryGetSpecialField(SpecialFieldFlags flag, out NodeFieldDisplay field)
        {
            if (_baseNode.TryGetSpecialField(flag, out NodeField baseField) && _fieldSection.Contains(baseField))
            {
                field = Fields[_fieldSection.VisualComponentList.IndexOf(baseField)] as NodeFieldDisplay;
                return true;
            }
            else
            {
                field = default;
                return false;
            }
        }
        */

        public FlowConnector GetFlowOutDisplayConnector()
        {
            if (_baseNode is IFlowNode flowNode && _fieldSection.VisualComponentList.Contains(flowNode.FlowOutField))
            {
                //int index = _fieldSection.VisualComponentList.IndexOf(flowNode.FlowOutField);
                return (flowNode.FlowOutField as VisualNodeComponent).OutputConnector.Value as FlowConnector;
            }

            return null;
        }

        public void TryEvaluate()
        {
            if (!_evaluating)
            {
                _evaluating = true;
                try
                {
                    _baseNode.Evaluate();
                    ErrorState = false;
                }
                catch
                {
                    ErrorState = true;
                }

                _evaluating = false;
            }
        }

        public void DeepUpdate()
        {
            foreach (VisualNodeComponent field in _fieldSection.VisualComponentList)
            {
                if (field.InputConnector is ValueConnector valInput && valInput.ExclusiveConnection != null)
                {
                    valInput.ExclusiveConnection.ParentNode.DeepUpdate();
                }
            }

            TryEvaluate();
        }
    }
}