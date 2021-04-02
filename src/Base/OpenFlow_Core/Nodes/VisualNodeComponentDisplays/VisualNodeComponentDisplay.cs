using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System.ComponentModel;

namespace OpenFlow_Core.Nodes.VisualNodeComponentDisplays
{
    public abstract class VisualNodeComponentDisplay<T> : IVisualNodeComponentDisplay
        where T : IVisualNodeComponent
    {
        public VisualNodeComponentDisplay(NodeBase parentNode, T childComponent)
        {
            ParentNode = parentNode;
            ChildComponent = childComponent;
            ChildComponent.GetFlowInput().PropertyChanged += (o, e) => UpdateInput();
            ChildComponent.GetFlowOutput().PropertyChanged += (o, e) => UpdateOutput();

            UpdateInput();
            UpdateOutput();
        }

        public IOpacity Opacity => ChildComponent.Opacity;

        public virtual IObservableValue<IConnector> InputConnector { get; } = Instance.Factory.GetImplementation<IObservableValue<IConnector>>();

        public IObservableValue<HorizontalAlignment> Alignment { get; } = Instance.Factory.GetImplementation<IObservableValue<HorizontalAlignment>>();

        public virtual IObservableValue<IConnector> OutputConnector { get; } = Instance.Factory.GetImplementation<IObservableValue<IConnector>>();

        protected T ChildComponent { get; }

        protected NodeBase ParentNode { get; }

        protected virtual bool TryUpdateConnector(IConnector connector, ConnectionType connectionType, out IConnector newConnector)
        {
            if (GetFlowFor(connectionType) && connector is not FlowConnector)
            {
                newConnector = new FlowConnector(ParentNode, connectionType);
                return true;
            }

            newConnector = default;
            return false;
        }

        protected void UpdateInput()
        {
            if (TryUpdateConnector(InputConnector.Value, ConnectionType.Input, out IConnector newInput))
            {
                InputConnector.Value = newInput;
            }
            Alignment.Value = CalculateAlignment();
        }

        protected void UpdateOutput()
        {
            if (TryUpdateConnector(OutputConnector.Value, ConnectionType.Output, out IConnector newOutput))
            {
                OutputConnector.Value = newOutput;
            }
            Alignment.Value = CalculateAlignment();
        }

        protected virtual HorizontalAlignment CalculateAlignment() => HorizontalAlignment.Middle;

        private bool GetFlowFor(ConnectionType connectionType) => (connectionType) switch
        {
            ConnectionType.Input => ChildComponent.GetFlowInput().Value,
            ConnectionType.Output => ChildComponent.GetFlowOutput().Value,
            _ => false,
        };
    }
}
