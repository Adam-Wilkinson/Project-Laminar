using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.Connection.ConnectorManagers
{
    public interface IConnectorManager
    {
        public static IEnumerable<Type> AllImplementingTypes { get; } = typeof(Connector).Assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(IConnectorManager)));

        event EventHandler ExistsChanged;

        IObservableValue<string> HexColour { get; }

        bool CheckIfConnectorExists(IVisualNodeComponent parentComponent, ConnectionType connectionType);

        void HookupExistsCheck(IVisualNodeComponent component, ConnectionType connectionType);

        bool ConnectorExclusiveCheck(ConnectionType connectionType);

        void ConnectionAddedAction(IConnector connection);

        void ConnectionRemovedAction(IConnector connection);

        bool CompatibilityCheck(IConnectorManager toCheck);
    }
}
