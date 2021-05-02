using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    public class SetWindowLayout : IActionNode
    {
        private readonly INodeField _layoutInput = Constructor.NodeField("Layout").WithInput<AllWindowsLayout>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _layoutInput;
            }
        }

        public string NodeName { get; } = "Set Window Layout";

        public void Evaluate()
        {
            AllWindowsLayout.SetCurrentLayout(_layoutInput.GetInput<AllWindowsLayout>());
        }
    }
}
