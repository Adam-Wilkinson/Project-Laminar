using System;
using System.Collections.Generic;
using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class ManualTriggerNode : ITriggerNode
    {
        private readonly INodeField _sourceField = Constructor.NodeField("Manual Trigger").WithValue("Displayed", Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(Constructor.TypeDefinition<Action>(null)), false);

        public IVisualNodeComponent FlowOutComponent => _sourceField;

        public string NodeName => "Flow Source";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _sourceField;
            }
        }

        public event EventHandler Trigger;

        public void HookupTriggers()
        {
            _sourceField["Displayed"] = (Action)(() =>
            {
                Trigger?.Invoke(this, new EventArgs());
            });
        }
    }
}
