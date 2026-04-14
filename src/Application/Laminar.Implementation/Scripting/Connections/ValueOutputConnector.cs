using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueOutputConnector<T>(ITypeInfoStore typeInfoStore, IValueOutput<T> output)
    : IOutputConnector<IValueOutput<T>>
{
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(Output.InterfaceDefinition.ValueType).HexColor;

    public IValueOutput<T> Output { get; } = output;

    public Action? PreEvaluateAction => Output.PreEvaluateAction;

    public void OnDisconnectedFrom(IInputConnector connector)
    {
    }

    public bool CanConnectTo(IInputConnector connector)
        => connector is IInputConnector<IValueInput<T>>; 
    
    public bool TryConnectTo(IInputConnector connector)
    {
        if (connector is not IInputConnector<IValueInput<T>> inputConnector) return false;
        inputConnector.Input.SetValueProvider(Output);
        return true;

    }

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        if (!executionFlags.HasValueFlag()) return PassUpdateOption.NeverPasses;
        return Output.AlwaysPassUpdate ? PassUpdateOption.AlwaysPasses : PassUpdateOption.CurrentlyDoesNotPass;
    }
}
