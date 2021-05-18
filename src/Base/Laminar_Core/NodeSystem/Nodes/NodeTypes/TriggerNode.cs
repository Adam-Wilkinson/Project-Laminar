using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class TriggerNode<T> : NodeContainer<T> where T : INode, new()
    {
        public TriggerNode(NodeDependencyAggregate dependencies) : base(dependencies)
        {
            BaseNode.GetNameLabel().WithFlowOutput();
        }

        public override T BaseNode
        {
            set
            {
                base.BaseNode = value;
                (BaseNode as ITriggerNode).Trigger += TriggerNode_Trigger;
            }
        }

        public override bool IsLive 
        {
            set
            {
                base.IsLive = value;
                if (IsLive)
                {
                    (BaseNode as ITriggerNode).HookupTriggers();
                }
                else
                {
                    (BaseNode as ITriggerNode).RemoveTriggers();
                }
            }
        }

        private void TriggerNode_Trigger(object sender, EventArgs e)
        {
            BaseNode.GetNameLabel().FlowOutput.Activate();
        }
    }
}
