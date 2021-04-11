using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Inbuilt.Nodes.Maths.Comparisons
{
    public class Equal : IFunctionNode
    {
        private readonly INodeField inputOne = Constructor.NodeField("First Input").WithInput(0.0);
        private readonly INodeField inputTwo = Constructor.NodeField("Second Input").WithInput(0.0);
        private readonly INodeField outputField = Constructor.NodeField("Equal").WithOutput(false);

        public string NodeName => "Equal";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return inputOne;
                yield return inputTwo;
                yield return outputField;
            }
        }

        public void Evaluate()
        {
            outputField.SetOutput(inputOne.GetInput().Equals(inputTwo.GetInput()));
        }
    }
}
