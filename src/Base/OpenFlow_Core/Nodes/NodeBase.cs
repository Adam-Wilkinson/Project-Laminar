﻿namespace OpenFlow_Core.Nodes
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
    using OpenFlow_PluginFramework.Primitives;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Nodes.NodeComponents.Collections;
    using OpenFlow_PluginFramework;
    using OpenFlow_Core.Nodes.NodeComponents.Visuals;

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

            TryEvaluate();
        }

        public IObservableValue<bool> ErrorState { get; }

        public double X { get; set; }

        public double Y { get; set; }

        public object Tag { get; set; }

        public string Name => _baseNode.NodeName;

        public INotifyCollectionChanged Fields => _fieldSection.VisualNodeComponentsObservable;

        public INodeBase DuplicateNode() => new NodeBase((INode)Activator.CreateInstance(_baseNode.GetType()), ErrorState.Clone());

        public FlowConnector GetFlowOutDisplayConnector()
        {
            if (_baseNode is IFlowNode flowNode && _fieldSection.VisualComponentList.Contains(flowNode.FlowOutComponent))
            {
                return (flowNode.FlowOutComponent as IDisplayableNodeComponent).ConnectionManager.OutputConnector.Value as FlowConnector;
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
            foreach (IDisplayableNodeComponent component in _fieldSection.VisualComponentList)
            {
                if (component.ConnectionManager.InputConnector is ValueConnector valInput && valInput.ExclusiveConnection != null)
                {
                    valInput.ExclusiveConnection.ParentNode.DeepUpdate();
                }
            }

            TryEvaluate();
        }
    }
}