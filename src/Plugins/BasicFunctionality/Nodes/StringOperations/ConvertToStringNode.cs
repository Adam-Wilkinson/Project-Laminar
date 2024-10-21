using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.StringOperations;

public partial class ConvertToStringNode : INode
{
    public string NodeName => "Convert to Text";

    public void Evaluate()
    {
        // converterField.SetOutput(converterField.GetInput() == null ? string.Empty : converterField.GetInput().ToString());
    }
}
