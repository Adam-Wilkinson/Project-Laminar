namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    using System.Collections;
    using System.Collections.Generic;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.Primitives;

    public abstract class VisualNodeComponent : NodeComponent, IVisualNodeComponent
    {
        public VisualNodeComponent(IObservableValue<string> name, IFlow flowInput, IFlow flowOutput, IOpacity opacity) : base(opacity)
        {
            Name = name;
            FlowInput = flowInput;
            FlowOutput = flowOutput;
            FlowInput.ParentComponent = this;
            FlowOutput.ParentComponent = this;

            VisualComponentList = new List<IVisualNodeComponent>() { this };
        }

        public virtual IObservableValue<string> Name { get; }

        public override IList VisualComponentList { get; }

        public IFlow FlowInput { get; }

        public IFlow FlowOutput { get; }

        protected override IVisualNodeComponent CloneTo(INodeComponent nodeField)
        {
            base.CloneTo(nodeField);
            (nodeField as IVisualNodeComponent).Name.Value = Name.Value;
            (nodeField as IVisualNodeComponent).FlowOutput.Exists = FlowOutput.Exists;
            (nodeField as IVisualNodeComponent).FlowInput.Exists = FlowInput.Exists;
            return nodeField as IVisualNodeComponent;
        }
    }
}
