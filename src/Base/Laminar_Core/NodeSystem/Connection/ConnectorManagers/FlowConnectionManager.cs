using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Connection.ConnectorManagers
{
    public class FlowConnectionManager : IConnectorManager
    {
        private readonly Instance _instance;
        private IVisualNodeComponent _parentComponent;
        private ConnectorType _connectorType;
        private FlowConnectionManager _pairedConnection;

        public FlowConnectionManager(Instance instance, IObservableValue<string> hexColour)
        {
            _instance = instance;
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
            INodeBase parentNodeBase = INodeBase.NodeBases[_parentComponent.ParentNode];

            if (_connectorType is ConnectorType.Input)
            {
                parentNodeBase.Update();
                FlashColourChange();
            }
            

            if (_connectorType is ConnectorType.Output && parentNodeBase.FlowOutContainer?.Child == _parentComponent)
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
            _instance.UIContext.Post(delegate { HexColour.Value = "#F000F0"; }, null);
            Task.Delay(new TimeSpan(0, 0, 0, 0, 300)).ContinueWith(t =>
            {
                _instance.UIContext.Post(delegate { HexColour.Value = "#800080"; }, null);
            });
        }
    }
}
