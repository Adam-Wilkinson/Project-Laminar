using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.StringOperations;

public class CharacterCounter : INode
{
    private readonly ValueInput<string> input = new("String", "");
    private readonly ValueOutput<double> output = new("Character Count", 0.0);

    public string NodeName { get; } = "Character Counter";

    public void Evaluate()
    {
        output.Value = input.Value.Length;
    }
}
