using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Benchmarks.BenchmarkNodes;

public class ValueAttributeBenchmarkNode : INode
{
    public readonly static List<ValueAttributeBenchmarkNode> Instances = new();

    public ValueAttributeBenchmarkNode()
    {
        Instances.Add(this);
    }

    [ValueInput<double>("input", TriggerEventName: nameof(InputChanged))] double Input { get; set; } = 0;
    [ValueOutput<double>("output")] public double Output { get; set; } = 0;

    public event EventHandler? InputChanged;

    public string NodeName { get; } = "Test Node";

    public void UpdateInput(double newValue)
    {
        Input = newValue;
        InputChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Evaluate()
    {
        Output = Input;
    }
}