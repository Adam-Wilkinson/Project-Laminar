using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.ComponentModel;

namespace OpenFlow_Core.Nodes.Connection.ConnectorManagers
{
    public class FlowConnectionManager : IConnectorManager
    {
        private IVisualNodeComponent _parentComponent;
        private ConnectorType _connectorType;

        public FlowConnectionManager(IObservableValue<string> hexColour)
        {
            HexColour = hexColour;
            HexColour.Value = "#800080";
        }

        public IObservableValue<string> HexColour { get; }

        public event EventHandler ExistsChanged;

        public void Initialize(IVisualNodeComponent component, ConnectorType connectionType)
        {
            _connectorType = connectionType;
            _parentComponent = component;

            if (connectionType is ConnectorType.Input)
            {
                component.GetFlowInput().PropertyChanged += FlowPropertyChanged;
            }

            if (connectionType is ConnectorType.Output)
            {
                component.GetFlowOutput().PropertyChanged += FlowPropertyChanged;
            }
        }

        public bool ConnectorExists()
        {
            if (_connectorType == ConnectorType.Input && _parentComponent.GetFlowInput().Value)
            {
                return true;
            }

            if (_connectorType == ConnectorType.Output && _parentComponent.GetFlowOutput().Value)
            {
                return true;
            }

            return false;
        }

        public bool CompatibilityCheck(IConnectorManager toCheck)
        {
            return toCheck is FlowConnectionManager;
        }

        public void ConnectionAddedAction(IConnectorManager manager)
        {
        }

        public void ConnectionRemovedAction(IConnectorManager manager)
        {
        }

        public bool ConnectorExclusiveCheck()
        {
            return _connectorType is ConnectorType.Output;
        }

        private void FlowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ExistsChanged?.Invoke(this, new EventArgs());
        }
    }
}
