namespace OpenFlow_Inbuilt.Nodes.Maths.Arithmetic
{
    using System.Collections.Generic;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework;

    public class NodeAdd : INode
    {
        private readonly INodeComponentAutoCloner addFields = Constructor.NodeComponentAutoCloner(Constructor.NodeField("Input").WithInput(0.0), 1, index => $"Number {index + 1}");
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
                total += (double)field.Input;
            }

            totalField.Output = total;
        }
    }
}
