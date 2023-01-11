using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Benchmarks.BenchmarkNodes;

public class ValueIOBenchmarNode : INode
{
    public readonly static List<ValueIOBenchmarNode> Instances = new();

    public ValueIOBenchmarNode()
    {
        Instances.Add(this);
    }

    public readonly ValueInput<double> Input = new("input", 0.0);

    public readonly ValueOutput<double> Output = new("output", 0.0);

    public string NodeName { get; } = "Test Node";

    public void Evaluate()
    {
        Output.Value = Input;
    }
}
