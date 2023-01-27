using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector : IInputConnector<IValueInput>
{
    readonly ITypeInfoStore _typeInfoStore;

    public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ValueInputConnector(ITypeInfoStore typeInfoStore)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _typeInfoStore = typeInfoStore;
    }

    public string ColorHex => Input.ValueType is null ? "#FFFFFF" : _typeInfoStore.GetTypeInfoOrBlank(Input.ValueType).HexColour;

    public IValueInput Input { get; private set; }

    public Action? PreEvaluateAction => Input.PreEvaluateAction;

    public void Init(IValueInput nodeInput)
    {
        Input = nodeInput;
    }

    public void OnDisconnectedFrom(IOutputConnector connector)
    {
        Input.TrySetValueProvider(null);
    }

    public bool TryConnectTo(IOutputConnector connector)
    {
        return connector is IOutputConnector<IValueOutput> outputConnector && Input.TrySetValueProvider(outputConnector.Output);
    }

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        return executionFlags.HasValueFlag() ? PassUpdateOption.AlwaysPasses : PassUpdateOption.NeverPasses;
    }
}
