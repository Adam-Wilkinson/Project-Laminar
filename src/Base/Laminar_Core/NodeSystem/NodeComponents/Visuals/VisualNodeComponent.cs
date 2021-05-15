namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    using System;
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

        public int IndexInParent { get; set; }

        public override void CloneTo(INodeComponent nodeComponent)
        {
            if (nodeComponent is not VisualNodeComponent visualNodeComponent)
            {
                throw new ArgumentException("VisualNodeComponent can only clone to another VisualNodeComponent");
            }

            base.CloneTo(nodeComponent);
            visualNodeComponent.Name.Value = Name.Value;
            visualNodeComponent.FlowOutput.Exists = FlowOutput.Exists;
            visualNodeComponent.FlowInput.Exists = FlowInput.Exists;
        }
    }
}
