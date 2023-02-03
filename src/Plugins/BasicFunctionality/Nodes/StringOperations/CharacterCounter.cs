using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;

namespace BasicFunctionality.Nodes.StringOperations;

public class CharacterCounter : INode
{
    private readonly ValueInputRow<string> input = new("String", "");
    private readonly ValueOutputRow<double> output = new("Character Count", 0.0);

    public string NodeName { get; } = "Character Counter";

    public void Evaluate()
    {
        output.Value = input.Value.Length;
    }
}
