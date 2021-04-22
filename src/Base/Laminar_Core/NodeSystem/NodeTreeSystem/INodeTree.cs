using Laminar_Core.NodeSystem.Connection;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    public interface INodeTree
    {
        ReadOnlyObservableCollection<INodeBase> Nodes { get; }

        public IObservableValue<string> Name { get; }

        INodeTreeInputs Inputs { get; }

        void AddNode(INodeBase newNode);

        IConnector GetActiveConnector(IConnector interacted);

        IEnumerable<INodeConnection> GetConnections();

        bool TryConnectFields(IConnector field1, IConnector field2);
    }
}