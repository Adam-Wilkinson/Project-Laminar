using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Editing.Connection.ConnectorManagers
{
    public class FlowConnectionManager : IConnectorManager
    {
        private readonly Instance _instance;
        private readonly List<FlowConnectionManager> _pairedConnections = new();
        private IVisualNodeComponent _parentComponent;
        private ConnectorType _connectorType;

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
                component.FlowInput.ExistsChanged += FlowChanged;
                component.FlowInput.Activated += FlowActivated;
            }

            if (connectionType is ConnectorType.Output)
            {
                component.FlowOutput.ExistsChanged += FlowChanged;
                component.FlowOutput.Activated += FlowActivated;
            }
        }

        private void FlowActivated(object sender, EventArgs e)
        {
            ActivateFlow(INodeContainer.GetNodeInstance((sender as IFlow).ParentComponent.ParentNode));
        }

        public void ActivateFlow(IAdvancedScriptInstance activatedInstance)
        {
            INodeContainer parentNodeBase = INodeContainer.NodeBases[_parentComponent.ParentNode];

            if (_connectorType is ConnectorType.Input)
            {
                FlashColourChange();
                if (_parentComponent == parentNodeBase.NameLabel)
                {
                    parentNodeBase.Update(activatedInstance);
                }
            }

            if (_connectorType is ConnectorType.Output)
            {
                FlashColourChange();
                foreach (FlowConnectionManager manager in _pairedConnections)
                {
                    manager._parentComponent.FlowInput.Activate();
                }

            }
        }

        public bool ConnectorExists()
        {
            if (_connectorType == ConnectorType.Input && _parentComponent.FlowInput.Exists)
            {
                return true;
            }

            if (_connectorType == ConnectorType.Output && _parentComponent.FlowOutput.Exists)
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
            _pairedConnections.Add(manager as FlowConnectionManager);
        }

        public void ConnectionRemovedAction(IConnectorManager manager)
        {
            _pairedConnections.Remove(manager as FlowConnectionManager);
        }

        public bool ConnectorExclusiveCheck()
        {
            return _connectorType is ConnectorType.Output;
        }

        public void Activate(IAdvancedScriptInstance instance, PropagationDirection direction)
        {
        }

        private void FlowChanged(object sender, bool exists)
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
