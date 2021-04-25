using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Inbuilt.Nodes.StringOperations
{
    public class CharacterCounter : IFunctionNode
    {
        private readonly INodeField counterField = Constructor.NodeField("Count").WithOutput<double>().WithInput<string>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return counterField;
            }
        }

        public string NodeName { get; } = "Character Counter";

        public void Evaluate()
        {
            counterField.SetOutput(Convert.ToDouble(counterField.GetInput<string>().Length));
        }
    }
}
