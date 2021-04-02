using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Inbuilt.Nodes.Input.MouseInput
{
    public class MouseButton : INode
    {
        private readonly INodeField mouseButtonOutput = Constructor.NodeField("Mouse Button ").WithValue("Display", MouseButtonEnum.LeftButton, true).WithOutput(MouseButtonEnum.LeftButton);

        public string NodeName => "Mouse Button";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return mouseButtonOutput;
            }
        }

        public void Evaluate()
        {
            mouseButtonOutput.Output = mouseButtonOutput["Display"];
        }
    }
}
