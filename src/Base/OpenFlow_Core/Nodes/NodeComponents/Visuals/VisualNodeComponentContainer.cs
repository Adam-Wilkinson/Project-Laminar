using OpenFlow_Core.Nodes.Connection;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public class VisualNodeComponentContainer : IVisualNodeComponentContainer
    {
        private IVisualNodeComponent _child;

        public VisualNodeComponentContainer(IConnector inputConnector, IConnector outputConnector)
        {
            InputConnector = inputConnector;
            OutputConnector = outputConnector;

            InputConnector.ConnectionType = ConnectorType.Input;
            OutputConnector.ConnectionType = ConnectorType.Output;
        }

        public IVisualNodeComponent Child
        {
            get => _child;
            set
            {
                _child = value;
                InputConnector.HookupRefreshTriggers(_child);
                OutputConnector.HookupRefreshTriggers(_child);
                HasRemoveFunction = _child.RemoveSelf is not null;
                RemoveAction = _child.RemoveSelf;
                Opacity = _child.Opacity;
            }
        }

        public IConnector InputConnector { get; }

        public IConnector OutputConnector { get; }

        public bool HasRemoveFunction { get; private set; }

        public Action RemoveAction { get; private set; }

        public IOpacity Opacity { get; private set; }
    }
}
