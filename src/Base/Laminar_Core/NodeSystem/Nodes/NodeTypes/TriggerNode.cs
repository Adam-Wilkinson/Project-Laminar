using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class TriggerNode<T> : NodeBase<T> where T : INode
    {
        public TriggerNode(NodeDependencyAggregate dependencies) : base(dependencies)
        {
            (BaseNode as ITriggerNode).Trigger += TriggerNode_Trigger;
            NameLabel.SetFlowOutput(true);
            FlowOutContainer = Name;
        }

        public override void MakeLive()
        {
            (BaseNode as ITriggerNode).HookupTriggers();
        }

        private void TriggerNode_Trigger(object sender, EventArgs e)
        {
             FlowOutContainer.OutputConnector?.Activate();
        }
    }
}
