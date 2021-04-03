using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenFlow_Core.Nodes
{
    public interface INodeBase
    {
        public static Dictionary<INode, INodeBase> NodeBases { get; } = new();

        IObservableValue<bool> ErrorState { get; }

        INotifyCollectionChanged Fields { get; }

        string Name { get; }

        object Tag { get; set; }

        double X { get; set; }

        double Y { get; set; }

        void DeepUpdate();

        INodeBase DuplicateNode();

        FlowConnector GetFlowOutDisplayConnector();

        void TryEvaluate();
    }
}