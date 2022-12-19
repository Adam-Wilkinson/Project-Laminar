using BenchmarkDotNet.Attributes;
using Laminar.Contracts.NodeSystem;
using Laminar.Core;
using Laminar.PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Benchmarks;

internal class ValuePassingBenchmark
{
    private readonly IScript _script;
    private readonly IScriptEditor _scriptEditor;
    private readonly INodeFactory _nodeWrapperFactory;
    Instance instance = new(null);

    public ValuePassingBenchmark()
    {
        _script = instance.ServiceProvider.GetService<IScriptFactory>()!.CreateScript();
        _scriptEditor = instance.ServiceProvider.GetService<IScriptEditor>()!;
        _nodeWrapperFactory = instance.ServiceProvider.GetService<INodeFactory>()!;

        INodeWrapper originalNode = _nodeWrapperFactory.WrapNode<TestNode>();
        var firstNode = _scriptEditor.AddCopyOfNode(_script, originalNode);
        var secondNode = _scriptEditor.AddCopyOfNode(_script, originalNode);
        var thirdNode = _scriptEditor.AddCopyOfNode(_script, originalNode);
        var fourthNode = _scriptEditor.AddCopyOfNode(_script, originalNode);
        _scriptEditor.TryBridgeConnectors(_script, firstNode.Fields[0].InputConnector!.NodeIOConnector, secondNode.Fields[1].OutputConnector!.NodeIOConnector);
        _scriptEditor.TryBridgeConnectors(_script, firstNode.Fields[0].InputConnector!.NodeIOConnector, thirdNode.Fields[1].OutputConnector!.NodeIOConnector);
        _scriptEditor.TryBridgeConnectors(_script, secondNode.Fields[0].InputConnector!.NodeIOConnector, fourthNode.Fields[1].OutputConnector!.NodeIOConnector);
    }

    public double PassValue(double newValue)
    {
        _script.Nodes[0].Fields[0].Display.ValueInfo.BoxedValue = newValue;
        return ((TestNode)_script.Nodes[3]).Output;
    }

    private class TestNode : INode
    {
        [ValueInput<double>("input")] public double Input { get; set; } = 0;
        [ValueOutput<double>("output")] public double Output { get; set; } = 0;

        public string NodeName { get; } = "Test Node";

        public void Evaluate()
        {
            Output = Input;
        }
    }
}
