using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenFlow_Core.NodeSystem.Nodes
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

        void Update();

        INodeBase DuplicateNode();
    }
}