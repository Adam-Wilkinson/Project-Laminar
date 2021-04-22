using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class TriggerNode<T> : NodeContainer<T> where T : INode
    {
        public TriggerNode(NodeDependencyAggregate dependencies) : base(dependencies)
        {
            NameLabel.SetFlowOutput(true);
            FlowOutContainer = Name;
        }

        public override T BaseNode
        {
            set
            {
                base.BaseNode = value;
                (BaseNode as ITriggerNode).Trigger += TriggerNode_Trigger;
            }
        }

        public override void MakeLive()
        {
            (BaseNode as ITriggerNode).HookupTriggers();
        }

        private void TriggerNode_Trigger(object sender, EventArgs e)
        {
             FlowOutContainer.OutputConnector?.Activate(null, Connection.PropagationDirection.Forwards);
        }
    }
}
