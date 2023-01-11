using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Functions;

public class NodeSine : INode
{
    private readonly ValueInput<double> inputField = new("x", 0.0);
    private readonly ValueOutput<double> outputField = new("Sin(x)", 0.0);

    public string NodeName => "Sine";

    public void Evaluate()
    {
        outputField.Value = inputField;
    }
}
