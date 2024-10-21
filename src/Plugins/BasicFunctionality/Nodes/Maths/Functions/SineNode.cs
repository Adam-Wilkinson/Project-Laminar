using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;

namespace BasicFunctionality.Nodes.Maths.Functions;

public partial class SineNode : INode
{
    //private readonly ValueInputRow<double> inputField = new("x", 0.0);
    //private readonly ValueOutputRow<double> outputField = new("Sin(x)", 0.0);

    public string NodeName => "Sine";

    public void Evaluate()
    {
        // outputField.Value = inputField;
    }
}
