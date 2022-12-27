using System.Transactions;
using BenchmarkDotNet.Attributes;
using Laminar.Contracts.NodeSystem;
using Laminar.Core;
using Laminar.PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Benchmarks;

[MemoryDiagnoser]
public class ValuePassingBenchmark
{
    private IScript _script1;
    private IScript _script2;
    private IScriptEditor _scriptEditor;
    private INodeFactory _nodeWrapperFactory;
    Instance instance = new(null, PluginFramework.Registration.FrontendDependency.None);

    private TestNodeProperties _firstNode;
    private TestNodeProperties _lastNode;

    private TestNodeFields _fieldsFirstNode;
    private TestNodeFields _fieldsLastNode;

    [GlobalSetup]
    public void Setup()
    {
        _script1 = instance.ServiceProvider.GetService<IScriptFactory>()!.CreateScript();
        _script2 = instance.ServiceProvider.GetService<IScriptFactory>()!.CreateScript();
        _scriptEditor = instance.ServiceProvider.GetService<IScriptEditor>()!;
        _nodeWrapperFactory = instance.ServiceProvider.GetService<INodeFactory>()!;

        SetupScript<TestNodeFields>(_script1);
        SetupScript<TestNodeProperties>(_script2);

        _firstNode = TestNodeProperties.Instances[1];
        _lastNode = TestNodeProperties.Instances[4];

        _fieldsFirstNode = TestNodeFields.Instances[1];
        _fieldsLastNode = TestNodeFields.Instances[4];
    }

    private void SetupScript<T>(IScript script) where T : INode, new()
    {
        INodeWrapper originalNode = _nodeWrapperFactory.WrapNode<T>();
        var firstNode = _scriptEditor.AddCopyOfNode(script, originalNode);
        var secondNode = _scriptEditor.AddCopyOfNode(script, originalNode);
        var thirdNode = _scriptEditor.AddCopyOfNode(script, originalNode);
        var fourthNode = _scriptEditor.AddCopyOfNode(script, originalNode);
        _scriptEditor.TryBridgeConnectors(script, firstNode.Fields[1].OutputConnector!.NodeIOConnector, secondNode.Fields[0].InputConnector!.NodeIOConnector);
        _scriptEditor.TryBridgeConnectors(script, firstNode.Fields[1].OutputConnector!.NodeIOConnector, thirdNode.Fields[0].InputConnector!.NodeIOConnector);
        _scriptEditor.TryBridgeConnectors(script, secondNode.Fields[1].OutputConnector!.NodeIOConnector, fourthNode.Fields[0].InputConnector!.NodeIOConnector);
        script.Nodes[0].Fields[0].Display.Value.Value = 3.0;
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

    [Benchmark]
    [Arguments(4)]
    public double PassValueProperties(double value)
    {
        _firstNode.UpdateInput(value);
        return _lastNode.Output;
    }

    private class TestNodeFields : INode
    {
        public static List<TestNodeFields> Instances = new();

        public TestNodeFields()
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


    private class TestNodeProperties : INode
    {
        public static List<TestNodeProperties> Instances = new();

        public TestNodeProperties()
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
}
