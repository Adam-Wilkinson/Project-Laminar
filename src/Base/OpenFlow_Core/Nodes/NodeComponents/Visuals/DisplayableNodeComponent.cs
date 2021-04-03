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

    public abstract class DisplayableNodeComponent : NodeComponent, IDisplayableNodeComponent
    {
        public DisplayableNodeComponent(IObservableValue<string> name, IOpacity opacity, IConnectionManager connectionManager) : base(opacity)
        {
            ConnectionManager = connectionManager;
            Name = name;

            ConnectionManager.AddConnectionCheck((connectionType) => FlowConnector.ConnectorCheck(this, connectionType));
            this.GetFlowInput().PropertyChanged += (o, e) => ConnectionManager.UpdateInput();
            this.GetFlowOutput().PropertyChanged += (o, e) => ConnectionManager.UpdateOutput();

            VisualComponentList = new List<IVisualNodeComponent>() { this };
        }

        public virtual IObservableValue<string> Name { get; }

        public override INode ParentNode 
        { 
            get => base.ParentNode;
            set
            {
                base.ParentNode = value;
                if (ParentNode != null)
                {
                    ConnectionManager.ParentNode = NodeBase.GetNodeBase(ParentNode);
                }
            }
        }

        public override IList VisualComponentList { get; }

        public IConnectionManager ConnectionManager { get; }

        protected override IVisualNodeComponent CloneTo(INodeComponent nodeField)
        {
            base.CloneTo(nodeField);
            (nodeField as IVisualNodeComponent).Name.Value = Name.Value;
            (nodeField as IVisualNodeComponent).SetFlowOutput(this.GetFlowOutput().Value);
            (nodeField as IVisualNodeComponent).SetFlowInput(this.GetFlowInput().Value);
            return nodeField as IVisualNodeComponent;
        }
    }
}
