using System;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector<T> : IInputConnector<IValueInput<T>>
{
    readonly ITypeInfoStore _typeInfoStore;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ValueInputConnector(ITypeInfoStore typeInfoStore)
    {
        _typeInfoStore = typeInfoStore;
    }

    public string ColorHex => _typeInfoStore.GetTypeInfoOrBlank(Input.InterfaceDefinition.ValueType).HexColor;

    public required IValueInput<T> Input { get; init; }

    public Action? PreEvaluateAction => Input.PreEvaluateAction;

    public void OnDisconnectedFrom(IOutputConnector connector)
    {
        Input.SetValueProvider(null);
    }

    public bool TryConnectTo(IOutputConnector connector)
    {
        if (connector is IOutputConnector<IValueOutput<T>> outputConnector)
        {
            Input.SetValueProvider(outputConnector.Output);
            return true;
        }

        return false;
    }

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        return executionFlags.HasValueFlag() ? PassUpdateOption.AlwaysPasses : PassUpdateOption.NeverPasses;
    }
}
