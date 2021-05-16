using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    public class SetWindowPos : IActionNode
    {
        private readonly INodeField _windowField = Constructor.NodeField("Window").WithInput<Window.Window>();
        private readonly INodeField _position = Constructor.NodeField("Position").WithInput<Rectangle>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _windowField;
                yield return _position;
            }
        }

        public string NodeName { get; } = "Set Window Position";

        public void Evaluate()
        {
            _windowField.GetInput<Window.Window>().Location = _position.GetInput<Rectangle>().Rect;
        }
    }
}
