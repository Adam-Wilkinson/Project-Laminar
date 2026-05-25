using BenchmarkDotNet.Attributes;
using Laminar.Benchmarks.BenchmarkNodes;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Extensions;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.UserInterface;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Benchmarks;

[MemoryDiagnoser]
public class ValuePassingBenchmark
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddLaminarServices(FrontendDependency.None, null)
        .BuildServiceProvider()
        .InitializeLaminar<Program>();
    
    private IScript? _script1;
    private IScriptEditor? _scriptEditor;
    private INodeFactory? _nodeWrapperFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ValueIoBenchmarkNode _fieldsFirstNode;
    private ValueIoBenchmarkNode _fieldsLastNode;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [GlobalSetup]
    public void Setup()
    {
        _script1 = _serviceProvider.GetService<IScriptFactory>()!.CreateScript();
        _scriptEditor = _serviceProvider.GetService<IScriptEditor>()!;
        _nodeWrapperFactory = _serviceProvider.GetService<INodeFactory>()!;

        SetupScript<ValueIoBenchmarkNode>(_script1, 500);

        _fieldsFirstNode = ValueIoBenchmarkNode.Instances[1];
        _fieldsLastNode = ValueIoBenchmarkNode.Instances[^1];
    }

    private void SetupScript<T>(IScript script, int nodeCount) where T : INode, new()
    {
        var originalNode = _nodeWrapperFactory!.WrapNode<T>();
        var previousNode = _scriptEditor!.AddMatchingNode(script, originalNode);
        for (var i = 0; i < nodeCount; i++)
        {
            var nextNode = _scriptEditor.AddMatchingNode(script, originalNode);
            _scriptEditor.FindBridgeConnectorsAction(script, previousNode.Rows[1].OutputConnector!, nextNode.Rows[0].InputConnector!);
            previousNode = nextNode;
        }
        (script.Nodes[0].Rows[0].CentralDisplay as IInterfaceData<double>)!.Value = 3.0;
        script.ExecutionInstance.IsShownInUI = false;
    }

    [Benchmark]
    [Arguments(4)]
    public double PassValueFields(double value)
    {
        _fieldsFirstNode.Output.Value = value;
        _fieldsFirstNode.Output.StartValueChangedExecution();
        return _fieldsLastNode.Output.Value;
    }
}
