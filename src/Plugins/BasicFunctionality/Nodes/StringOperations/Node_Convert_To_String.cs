using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.StringOperations;

public class Node_Convert_To_String : INode
{
    public string NodeName => "Convert to Text";

    public void Evaluate()
    {
        // converterField.SetOutput(converterField.GetInput() == null ? string.Empty : converterField.GetInput().ToString());
    }
}
