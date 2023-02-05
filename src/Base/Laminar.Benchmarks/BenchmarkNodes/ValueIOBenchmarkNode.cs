using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;

namespace Laminar.Benchmarks.BenchmarkNodes;

public class ValueIOBenchmarNode : INode
{
    public readonly static List<ValueIOBenchmarNode> Instances = new();

    public ValueIOBenchmarNode()
    {
        Instances.Add(this);
    }

    [ShowInNode] readonly ValueInputRow<double> Input = new("input", 0.0);

    [ShowInNode] readonly ValueOutputRow<double> Output = new("output", 0.0);

    public string NodeName { get; } = "Test Node";

    public void Evaluate()
    {
        Output.Value = Input;
    }
}
