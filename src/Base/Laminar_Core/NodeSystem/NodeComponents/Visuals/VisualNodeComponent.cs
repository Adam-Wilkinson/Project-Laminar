namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    using System.Collections;
    using System.Collections.Generic;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.Primitives;

    public abstract class VisualNodeComponent : NodeComponent, IVisualNodeComponent
    {
        public VisualNodeComponent(IObservableValue<string> name, IOpacity opacity) : base(opacity)
        {
            Name = name;

            VisualComponentList = new List<IVisualNodeComponent>() { this };
        }

        public virtual IObservableValue<string> Name { get; }

        public override IList VisualComponentList { get; }

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
