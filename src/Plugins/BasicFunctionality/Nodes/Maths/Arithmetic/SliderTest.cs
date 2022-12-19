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

namespace BasicFunctionality.Nodes.Maths.Arithmetic
{
    public class SliderTest : IFunctionNode
    {
        private readonly INodeField _sliderTest = Constructor.NodeField("Slider Test").WithValue("Display", Constructor.RigidTypeDefinitionManager(50.0, "SliderEditor", null), true);

        public string NodeName { get; } = "Slider Test";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _sliderTest;
            }
        }

        public void Evaluate()
        {
        }
    }
}
