using OpenFlow_Core.NodeSystem.Nodes;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OpenFlow_Core.NodeSystem.Connection.ConnectorManagers
{
    public class FlowConnectionManager : IConnectorManager
    {
        private IVisualNodeComponent _parentComponent;
        private ConnectorType _connectorType;
        private FlowConnectionManager _pairedConnection;

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
            _pairedConnection = manager as FlowConnectionManager;
        }

        public void ConnectionRemovedAction(IConnectorManager manager)
        {
            _pairedConnection = null;
        }

        public bool ConnectorExclusiveCheck()
        {
            return _connectorType is ConnectorType.Output;
        }

        public void Activate()
        {
            if (_connectorType is ConnectorType.Input)
            {
                INodeBase.NodeBases[_parentComponent.ParentNode].Update();
                FlashColourChange();
            }
            

            if (_connectorType is ConnectorType.Output && _parentComponent.ParentNode is IFlowNode parentFlowNode && parentFlowNode.FlowOutComponent == _parentComponent)
            {
                FlashColourChange();
                _pairedConnection?.Activate();
            }
        }

        private void FlowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ExistsChanged?.Invoke(this, new EventArgs());
        }

        private void FlashColourChange()
        {
            HexColour.Value = "#F000F0";
            Task.Delay(new TimeSpan(0, 0, 0, 0, 300)).ContinueWith(t =>
            {
                Instance.Current.UIContext.Post(delegate { HexColour.Value = "#800080"; }, null);
            });
        }
    }
}
