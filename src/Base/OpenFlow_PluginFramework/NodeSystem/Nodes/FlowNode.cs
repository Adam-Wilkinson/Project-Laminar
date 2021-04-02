namespace OpenFlow_PluginFramework.NodeSystem.Nodes
{
    using System;
    using System.Collections.Generic;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

    public abstract class FlowNode : IFlowNode
    {
        private readonly INodeLabel _flowField = Constructor.NodeLabel("Flow").WithFlowInput().WithFlowOutput();

        public abstract string NodeName { get; }

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _flowField;
                foreach (INodeComponent field in FlowNodeFields)
                {
                    yield return field;
                }
            }
        }

        public IVisualNodeComponent FlowOutField => _flowField;

        protected abstract IEnumerable<INodeComponent> FlowNodeFields { get; }

        public abstract void Evaluate();
    }
}
