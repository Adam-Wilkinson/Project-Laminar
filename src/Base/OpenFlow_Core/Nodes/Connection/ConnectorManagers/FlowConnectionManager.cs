using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.ComponentModel;

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

        public ConnectorType ConnectionType { get; private set; }

        public event EventHandler ExistsChanged;

        public bool CheckIfConnectorExists(IVisualNodeComponent parentComponent, ConnectorType connectionType)
        {
            if (connectionType == ConnectorType.Input && parentComponent.GetFlowInput().Value)
            {
                ConnectionType = connectionType;
                return true;
            }

            if (connectionType == ConnectorType.Output && parentComponent.GetFlowOutput().Value)
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

        public void ConnectionAddedAction(IConnectorManager manager, ConnectorType connectorType)
        {
        }

        public void ConnectionRemovedAction(IConnectorManager manager, ConnectorType connectorType)
        {
        }

        public bool ConnectorExclusiveCheck(ConnectorType connectionType)
        {
            return connectionType is ConnectorType.Output;
        }

        public void HookupExistsCheck(IVisualNodeComponent component, ConnectorType connectionType)
        {
            if (connectionType is ConnectorType.Input)
            {
                component.GetFlowInput().PropertyChanged += FlowPropertyChanged;
            }

            if (connectionType is ConnectorType.Output)
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
