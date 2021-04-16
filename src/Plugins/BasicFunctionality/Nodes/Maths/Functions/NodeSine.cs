namespace Laminar_Inbuilt.Nodes.Maths.Functions
{
    using System;
    using System.Collections.Generic;
    using Laminar_PluginFramework;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    
    public class NodeSine : IFunctionNode
    {
        private readonly INodeField inputField = Constructor.NodeField("x").WithInput(0.0);
        private readonly INodeField outputField = Constructor.NodeField("sin(x)").WithOutput(0.0);

        public string NodeName => "Sine";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return inputField;
                yield return outputField;
            }
        }

        public void Evaluate()
        {
            outputField.SetOutput(Math.Sin(inputField.GetInput<double>()));
        }
    }
}
