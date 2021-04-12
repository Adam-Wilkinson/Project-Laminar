using OpenFlow_Core.Primitives;
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

        IObservableValue<string> Name { get; }

        INotifyCollectionChanged Fields { get; }

        IPoint Location { get; }

        void Update();

        INodeBase DuplicateNode();
    }
}