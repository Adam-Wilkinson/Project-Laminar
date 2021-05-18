namespace Laminar_Inbuilt.Nodes.Maths.Arithmetic
{
    using System.Collections.Generic;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework;

    public class NodeAdd : IFunctionNode
    {
        // private readonly INodeComponentAutoCloner addFields = Constructor.NodeComponentAutoCloner(Constructor.NodeField("Input").WithInput(0.0), 1, index => $"Number {index + 1}");
        private readonly INodeComponentList addFields = Constructor.NodeComponentList(Constructor.NodeField("Input 1").WithInput(0.0), Constructor.NodeField("Input 2").WithInput(0.0));
        private readonly INodeField totalField = Constructor.NodeField("Output").WithOutput(0.0);

        public string NodeName { get; } = "Add";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return addFields;
                yield return totalField;
            }
        }

        public void Evaluate()
        {
            double total = 0;

            foreach (INodeField field in addFields)
            {
                total += field.GetInput<double>();
            }

            totalField.SetOutput(total);
        }
    }
}
