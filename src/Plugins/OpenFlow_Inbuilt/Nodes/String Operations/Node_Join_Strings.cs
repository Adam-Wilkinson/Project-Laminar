namespace OpenFlow_Inbuilt.Nodes.StringOperations
{
    using System.Collections.Generic;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework;

    public class Node_Join_Strings : INode
    {
        private readonly INodeComponentAutoCloner combineStrings = Constructor.NodeComponentAutoCloner(Constructor.NodeField("Text").WithInput(""), 2, (x) => $"Text {x}");
        private readonly INodeField combinedString = Constructor.NodeField("Combined").WithOutput("");

        public string NodeName => "Join Strings";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return combineStrings;
                yield return combinedString;
            }
        }

        public void Evaluate()
        {
            string output = string.Empty;
            foreach (INodeField field in combineStrings)
            {
                output += field.GetInput<string>();
            }

            combinedString.SetOutput(output);
        }
    }
}
