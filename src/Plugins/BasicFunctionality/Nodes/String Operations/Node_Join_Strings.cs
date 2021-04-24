namespace Laminar_Inbuilt.Nodes.StringOperations
{
    using System.Collections.Generic;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework;

    public class Node_Join_Strings : IFunctionNode
    {
        private readonly INodeComponentAutoCloner combineStrings = Constructor.NodeComponentAutoCloner(Constructor.NodeField("Text").WithInput(""), 2, (x) => $"Text {x + 1}");
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
