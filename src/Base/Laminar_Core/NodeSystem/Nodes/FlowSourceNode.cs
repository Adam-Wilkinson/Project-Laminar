using System;
using System.Collections.Generic;
using Laminar_PluginFramework;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar_Core.NodeSystem.Nodes
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
