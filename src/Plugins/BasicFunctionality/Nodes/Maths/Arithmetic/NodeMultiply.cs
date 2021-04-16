namespace Laminar_Inbuilt.Nodes.Maths.Arithmetic
{
    using System.Collections.Generic;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework;

    public class NodeMultiply : IFunctionNode
    {
        private readonly INodeComponentAutoCloner multiplyFields = Constructor.NodeComponentAutoCloner(Constructor.NodeField("Number").WithInput(1.0), 1, index => $"Number {index + 1}");
        private readonly INodeField outputField = Constructor.NodeField("Product").WithOutput(0.0);

        public string NodeName => "Multiply";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return multiplyFields;
                yield return outputField;
            }
        }

        public void Evaluate()
        {
            double output = 1.0;

            foreach (INodeField field in multiplyFields)
            {
                output *= field.GetInput<double>();
            }

            outputField.SetOutput(output);
        }
    }
}
