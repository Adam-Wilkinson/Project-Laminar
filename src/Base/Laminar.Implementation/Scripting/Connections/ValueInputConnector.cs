using System;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector : IInputConnector<IValueInput>
{
    readonly ITypeInfoStore _typeInfoStore;
    IValueInput? _input;

    public ValueInputConnector(ITypeInfoStore typeInfoStore)
    {
        _typeInfoStore = typeInfoStore;
    }

    public string ColorHex => _input.ValueType is null ? "#FFFFFF" : _typeInfoStore.GetTypeInfoOrBlank(_input.ValueType).HexColour;

    public IValueInput Input => _input;

    public Action PreEvaluateAction => _input.PreEvaluateAction;

    public void Init(IValueInput nodeInput)
    {
        _input = nodeInput;
    }

    public void OnDisconnectedFrom(IOutputConnector connector)
    {
        _input.TrySetValueProvider(null);
    }

    public bool TryConnectTo(IOutputConnector connector)
    {
        return connector is IOutputConnector<IValueOutput> outputConnector && _input.TrySetValueProvider(outputConnector.Output);
    }

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        return executionFlags.HasValueFlag() ? PassUpdateOption.AlwaysPasses : PassUpdateOption.NeverPasses;
    }
}
