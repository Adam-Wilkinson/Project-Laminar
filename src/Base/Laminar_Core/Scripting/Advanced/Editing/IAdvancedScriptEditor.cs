using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public interface IAdvancedScriptEditor
    {
        public bool IsLive { get; set; }

        ReadOnlyObservableCollection<INodeContainer> Nodes { get; }

        void AddNode(INodeContainer newNode);

        IConnector GetActiveConnector(IConnector interacted);

        IEnumerable<INodeConnection> Connections { get; }

        bool TryConnectFields(IConnector field1, IConnector field2);

        void DeleteNode(INodeContainer coreNode);
    }
}
