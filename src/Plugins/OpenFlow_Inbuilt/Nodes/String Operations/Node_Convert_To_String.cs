namespace Laminar_Inbuilt.Nodes.StringOperations
{
    using System.Collections.Generic;
    using Laminar_PluginFramework;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework.Primitives.TypeDefinition;

    public class Node_Convert_To_String : IFunctionNode
    {
        private readonly INodeField converterField = Constructor.NodeField("Text")
            .WithInput(Constructor.TypeDefinitionManager())
            .WithOutput("");

        public string NodeName => "Convert to Text";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return converterField;
            }
        }

        public void Evaluate()
        {
            converterField.SetOutput(converterField.GetInput() == null ? string.Empty : converterField.GetInput().ToString());
        }
    }
}
