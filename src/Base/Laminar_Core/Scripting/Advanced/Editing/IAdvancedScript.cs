using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public interface IAdvancedScript
    {
        ReadOnlyObservableCollection<INodeContainer> Nodes { get; }

        public bool EditorIsLive { get; set; }

        public IObservableValue<string> Name { get; }

        IAdvancedScriptInputs Inputs { get; }

        void AddNode(INodeContainer newNode);

        IConnector GetActiveConnector(IConnector interacted);

        IEnumerable<INodeConnection> GetConnections();

        bool TryConnectFields(IConnector field1, IConnector field2);

        void DeleteNode(INodeContainer coreNode);
    }
}