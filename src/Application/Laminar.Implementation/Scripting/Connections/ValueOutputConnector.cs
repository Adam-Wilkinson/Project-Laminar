using System;
using System.Collections.Generic;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueOutputConnector<T> : IOutputConnector<IValueOutput<T>>
{
    readonly ITypeInfoStore _typeInfoStore;

    public ValueOutputConnector(ITypeInfoStore typeInfoStore, IValueOutput<T> output)
    {
        _typeInfoStore = typeInfoStore;
        Output = output;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ColorHex => _typeInfoStore.GetTypeInfoOrBlank(Output.InterfaceDefinition.ValueType).HexColor;

    public IValueOutput<T> Output { get; }

    public Action? PreEvaluateAction => Output.PreEvaluateAction;

    public void OnDisconnectedFrom(IInputConnector connector)
    {
    }

    public bool TryConnectTo(IInputConnector connector)
    {
        if (connector is IInputConnector<IValueInput<T>> inputConnector)
        {
            inputConnector.Input.SetValueProvider(Output);
            return true;
        }

        return false;
    }

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        if (executionFlags.HasValueFlag())
        {
            if (Output.AlwaysPassUpdate)
            {
                return PassUpdateOption.AlwaysPasses;
            }

            return PassUpdateOption.CurrentlyDoesNotPass;
        }

        return PassUpdateOption.NeverPasses;
    }
}
