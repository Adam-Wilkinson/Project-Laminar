namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Primitives;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;

    public abstract class VisualNodeComponent : NodeComponent, IVisualNodeComponent
    {
        private string _name;
        private NodeBase _parentNodeBase;

        public VisualNodeComponent(IOpacity opacity) : base(opacity)
        {
            VisualComponentList = new List<IVisualNodeComponent>() { this };
            this.GetFlowInput().PropertyChanged += (o, e) => UpdateInput();
            this.GetFlowOutput().PropertyChanged += (o, e) => UpdateOutput();

            UpdateInput();
            UpdateOutput();
        }

        public virtual string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override IList VisualComponentList { get; }

        public virtual IObservableValue<IConnector> InputConnector { get; } = Instance.Factory.GetImplementation<IObservableValue<IConnector>>();

        public IObservableValue<HorizontalAlignment> Alignment { get; } = Instance.Factory.GetImplementation<IObservableValue<HorizontalAlignment>>();

        public virtual IObservableValue<IConnector> OutputConnector { get; } = Instance.Factory.GetImplementation<IObservableValue<IConnector>>();

        protected override IVisualNodeComponent CloneTo(INodeComponent nodeField)
        {
            base.CloneTo(nodeField);
            (nodeField as IVisualNodeComponent).Name = Name;
            (nodeField as IVisualNodeComponent).SetFlowOutput(this.GetFlowOutput().Value);
            (nodeField as IVisualNodeComponent).SetFlowInput(this.GetFlowInput().Value);
            return nodeField as IVisualNodeComponent;
        }

        protected NodeBase ParentNodeBase
        {
            get => _parentNodeBase;
            set
            {
                _parentNodeBase = value;
                UpdateInput();
                UpdateOutput();
            }
        }

        protected virtual bool TryUpdateConnector(IConnector connector, ConnectionType connectionType, out IConnector newConnector)
        {
            if (GetFlowFor(connectionType) && connector is not FlowConnector)
            {
                newConnector = new FlowConnector(ParentNodeBase, connectionType);
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
            ConnectionType.Input => this.GetFlowInput().Value,
            ConnectionType.Output => this.GetFlowOutput().Value,
            _ => false,
        };
    }
}
