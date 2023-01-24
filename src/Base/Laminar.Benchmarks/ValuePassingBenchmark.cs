using BenchmarkDotNet.Attributes;
using Laminar.Benchmarks.BenchmarkNodes;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation;
using Laminar.PluginFramework.NodeSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Benchmarks;

[MemoryDiagnoser]
public class ValuePassingBenchmark
{
    private IScript _script1;
    private IScript _script2;
    private IScriptEditor _scriptEditor;
    private INodeFactory _nodeWrapperFactory;
    readonly Instance instance = new(null, PluginFramework.Registration.FrontendDependency.None);

    //private ValueAttributeBenchmarkNode _firstNode;
    //private ValueAttributeBenchmarkNode _lastNode;

    private ValueIOBenchmarNode _fieldsFirstNode;
    private ValueIOBenchmarNode _fieldsLastNode;

    [GlobalSetup]
    public void Setup()
    {
        _script1 = instance.ServiceProvider.GetService<IScriptFactory>()!.CreateScript();
        _script2 = instance.ServiceProvider.GetService<IScriptFactory>()!.CreateScript();
        _scriptEditor = instance.ServiceProvider.GetService<IScriptEditor>()!;
        _nodeWrapperFactory = instance.ServiceProvider.GetService<INodeFactory>()!;

        SetupScript<ValueIOBenchmarNode>(_script1, 5);
        //SetupScript<ValueAttributeBenchmarkNode>(_script2, 5);

        //_firstNode = ValueAttributeBenchmarkNode.Instances[1];
        //_lastNode = ValueAttributeBenchmarkNode.Instances[^1];

        _fieldsFirstNode = ValueIOBenchmarNode.Instances[1];
        _fieldsLastNode = ValueIOBenchmarNode.Instances[^1];
    }

    private void SetupScript<T>(IScript script, int nodeCount) where T : INode, new()
    {
        IWrappedNode originalNode = _nodeWrapperFactory.WrapNode<T>();
        var previousNode = _scriptEditor.AddCopyOfNode(script, originalNode);
        for (int i = 0; i < nodeCount; i++)
        {
            var nextNode = _scriptEditor.AddCopyOfNode(script, originalNode);
            _scriptEditor.TryBridgeConnectors(script, previousNode.Rows[1].OutputConnector!.NodeIOConnector, nextNode.Rows[0].InputConnector!.NodeIOConnector);
            previousNode = nextNode;
        }
        script.Nodes[0].Rows[0].Display.Value.Value = 3.0;
        script.ExecutionInstance.IsShownInUI = false;
    }

    public static double TestRun()
    {
        var testInstance = new ValuePassingBenchmark();
        testInstance.Setup();
        return testInstance.PassValueFields(4);
    }

    [Benchmark]
    [Arguments(4)]
    public double PassValueFields(double value)
    {
        _fieldsFirstNode.Input.SetInternalValue(value);
        return _fieldsLastNode.Output.Value;
    }
}
