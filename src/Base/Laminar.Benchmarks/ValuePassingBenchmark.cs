using BenchmarkDotNet.Attributes;
using Laminar.Benchmarks.BenchmarkNodes;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation;
using Laminar.Implementation.Scripting.NodeIO;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Benchmarks;

[MemoryDiagnoser]
public class ValuePassingBenchmark
{
    private IScript? _script1;
    private IScriptEditor? _scriptEditor;
    private INodeFactory? _nodeWrapperFactory;
    readonly Instance instance = new(null, PluginFramework.Registration.FrontendDependency.None);

    //private ValueAttributeBenchmarkNode _firstNode;
    //private ValueAttributeBenchmarkNode _lastNode;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ValueInput<double> _firstInput;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ValueIOBenchmarNode _fieldsFirstNode;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private ValueIOBenchmarNode _fieldsLastNode;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [GlobalSetup]
    public void Setup()
    {
        _script1 = instance.ServiceProvider.GetService<IScriptFactory>()!.CreateScript();
        _scriptEditor = instance.ServiceProvider.GetService<IScriptEditor>()!;
        _nodeWrapperFactory = instance.ServiceProvider.GetService<INodeFactory>()!;

        SetupScript<ValueIOBenchmarNode>(_script1, 500);

        _fieldsFirstNode = ValueIOBenchmarNode.Instances[1];
        _fieldsLastNode = ValueIOBenchmarNode.Instances[^1];
    }

    private void SetupScript<T>(IScript script, int nodeCount) where T : INode, new()
    {
        IWrappedNode originalNode = _nodeWrapperFactory!.WrapNode<T>();
        IWrappedNode previousNode = _scriptEditor!.AddCopyOfNode(script, originalNode);
        for (int i = 0; i < nodeCount; i++)
        {
            IWrappedNode nextNode = _scriptEditor.AddCopyOfNode(script, originalNode);
            _scriptEditor.TryBridgeConnectors(script, previousNode.Rows[1].OutputConnector!, nextNode.Rows[0].InputConnector!);
            previousNode = nextNode;
        }
        (script.Nodes[0].Rows[0].CentralDisplay as IDisplay)!.DisplayValue.Value = 3.0;
        script.ExecutionInstance.IsShownInUI = false;
    }

    public static double TestRun()
    {
        ValuePassingBenchmark testInstance = new();
        testInstance.Setup();
        return testInstance.PassValueFields(4);
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
