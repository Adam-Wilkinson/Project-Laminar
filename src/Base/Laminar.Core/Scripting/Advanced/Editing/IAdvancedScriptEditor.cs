using Laminar.Contracts.NodeSystem;
using Laminar.Domain.ValueObjects;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public interface IAdvancedScriptEditor
    {
        public bool IsLive { get; set; }

        ReadOnlyObservableCollection<INodeWrapper> Nodes { get; }

        IAdvancedScriptInputs Inputs { get; }

        IEnumerable<INodeWrapper> TriggerNodes { get; }

        void AddNode(INodeWrapper newNode);

        INodeWrapper GetNode(Identifier<INodeWrapper> nodeGuid);

        Connection.IConnector GetActiveConnector(Connection.IConnector interacted);

        IEnumerable<INodeConnection> Connections { get; }

        bool TryConnectFields(Connection.IConnector field1, Connection.IConnector field2, out INodeConnection connection);

        void DeleteNode(INodeWrapper coreNode);
    }
}
