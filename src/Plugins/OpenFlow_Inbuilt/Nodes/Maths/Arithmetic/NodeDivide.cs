namespace OpenFlow_Inbuilt.Nodes.Maths.Arithmetic
{
    using System.Collections.Generic;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;

    public class NodeDivide : IFunctionNode
    {
        private readonly INodeField firstNumber = Constructor.NodeField("Number 1").WithInput(0.0);
        private readonly INodeField secondNumber = Constructor.NodeField("Number 2").WithInput(1.0);
        private readonly INodeField outputField = Constructor.NodeField("Result").WithOutput(0.0);

        public string NodeName => "Divide";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return firstNumber;
                yield return secondNumber;
                yield return outputField;
            }
        }

        public void Evaluate()
        {
            outputField.SetOutput(firstNumber.GetInput<double>() / secondNumber.GetInput<double>());
        }
    }
}
