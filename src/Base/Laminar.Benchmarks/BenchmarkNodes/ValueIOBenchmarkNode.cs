using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using static Laminar.PluginFramework.LaminarFactory;

namespace Laminar.Benchmarks.BenchmarkNodes;

public class ValueIOBenchmarNode : INode
{
    public readonly static List<ValueIOBenchmarNode> Instances = new();

    public ValueIOBenchmarNode()
    {
        Instances.Add(this);
    }

    [ShowInNode] public readonly ValueInputRow<double> Input = Component.ValueInput("input", 0.0);

    [ShowInNode] public readonly ValueOutputRow<double> Output = Component.ValueOutput("output", 0.0);

    public string NodeName { get; } = "Test Node";

    public void Evaluate()
    {
        Output.Value = Input;
    }
}
