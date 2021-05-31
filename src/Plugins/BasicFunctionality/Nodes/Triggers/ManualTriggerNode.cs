using System;
using System.Collections.Generic;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar_Inbuilt.Nodes.Triggers
{
    public class ManualTriggerNode : ITriggerNode
    {
        private readonly INodeField _sourceField = Constructor.NodeField("Trigger").WithValue("Displayed", (Action)(() => { }), false);

        public IVisualNodeComponent FlowOutComponent => _sourceField;

        public string NodeName => "Manual Trigger";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _sourceField;
            }
        }

        public event EventHandler Trigger;

        public void RemoveTriggers()
        {
            _sourceField["Displayed"] = (Action)(() => { });
        }

        public void HookupTriggers()
        {
            _sourceField["Displayed"] = (Action)(() =>
            {
                Trigger?.Invoke(this, new EventArgs());
            });
        }
    }
}
