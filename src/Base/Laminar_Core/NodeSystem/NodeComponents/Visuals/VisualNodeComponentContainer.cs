using Laminar_Core.NodeSystem.Connection;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class VisualNodeComponentContainer : IVisualNodeComponentContainer
    {
        private IVisualNodeComponent _child;

        public VisualNodeComponentContainer(IConnector inputConnector, IConnector outputConnector)
        {
            InputConnector = inputConnector;
            OutputConnector = outputConnector;

            InputConnector.ConnectorType = ConnectorType.Input;
            OutputConnector.ConnectorType = ConnectorType.Output;
        }

        public IVisualNodeComponent Child
        {
            get => _child;
            set
            {
                _child = value;
                InputConnector.Initialize(_child);
                OutputConnector.Initialize(_child);
                HasRemoveFunction = _child.RemoveAction is not null;
                RemoveAction = () =>
                {
                    _child.RemoveAction(_child);
                    INodeBase.NodeBases[_child.ParentNode].Update();
                };
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
