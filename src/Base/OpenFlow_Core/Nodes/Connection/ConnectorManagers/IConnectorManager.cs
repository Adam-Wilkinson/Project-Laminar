using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFlow_Core.Nodes.Connection.ConnectorManagers
{
    public interface IConnectorManager
    {
        public static IEnumerable<Type> AllImplementingTypes { get; } = typeof(Connector).Assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(IConnectorManager)));

        event EventHandler ExistsChanged;

        IObservableValue<string> HexColour { get; }

        bool CheckIfConnectorExists(IVisualNodeComponent parentComponent, ConnectorType connectionType);

        void HookupExistsCheck(IVisualNodeComponent component, ConnectorType connectionType);

        bool ConnectorExclusiveCheck(ConnectorType connectorType);

        void ConnectionAddedAction(IConnectorManager manager, ConnectorType connectorType);

        void ConnectionRemovedAction(IConnectorManager manager, ConnectorType connectorType);

        bool CompatibilityCheck(IConnectorManager toCheck);
    }
}
