using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.Connection.ConnectorManagers
{
    public class FlowConnectionManager : IConnectorManager
    {
        public FlowConnectionManager(IObservableValue<string> hexColour)
        {
            HexColour = hexColour;
            HexColour.Value = "#800080";
        }

        public IObservableValue<string> HexColour { get; }

        public ConnectionType ConnectionType { get; private set; }

        public event EventHandler ExistsChanged;

        public bool CheckIfConnectorExists(IVisualNodeComponent parentComponent, ConnectionType connectionType)
        {
            if (connectionType == ConnectionType.Input && parentComponent.GetFlowInput().Value)
            {
                ConnectionType = connectionType;
                return true;
            }

            if (connectionType == ConnectionType.Output && parentComponent.GetFlowOutput().Value)
            {
                ConnectionType = connectionType;
                return true;
            }

            return false;
        }

        public bool CompatibilityCheck(IConnectorManager toCheck)
        {
            return toCheck is FlowConnectionManager;
        }

        public void ConnectionAddedAction(IConnector connection)
        {
        }

        public void ConnectionRemovedAction(IConnector connection)
        {
        }

        public bool ConnectorExclusiveCheck(ConnectionType connectionType)
        {
            return connectionType is ConnectionType.Output;
        }

        public void HookupExistsCheck(IVisualNodeComponent component, ConnectionType connectionType)
        {
            if (connectionType is ConnectionType.Input)
            {
                component.GetFlowInput().PropertyChanged += FlowPropertyChanged;
            }

            if (connectionType is ConnectionType.Output)
            {
                component.GetFlowOutput().PropertyChanged += FlowPropertyChanged;
            }
        }

        private void FlowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ExistsChanged?.Invoke(this, new EventArgs());
        }
    }
}
