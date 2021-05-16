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
    public class GetWindowPos : IFunctionNode
    {
        private readonly INodeField _mainField = Constructor.NodeField("Window").WithInput<Window.Window>().WithOutput<Rectangle>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _mainField;
            }
        }

        public string NodeName { get; } = "Get Window Position";

        public void Evaluate()
        {
            _mainField.GetOutput<Rectangle>().Rect = _mainField.GetInput<Window.Window>().Location;
        }
    }
}
