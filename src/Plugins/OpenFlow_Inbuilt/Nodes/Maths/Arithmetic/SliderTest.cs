using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Inbuilt.Nodes.Maths.Arithmetic
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
