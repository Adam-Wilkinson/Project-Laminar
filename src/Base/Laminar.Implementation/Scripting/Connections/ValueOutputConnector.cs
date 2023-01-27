using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueOutputConnector : IOutputConnector<IValueOutput>
{
    readonly ITypeInfoStore _typeInfoStore;

    public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ValueOutputConnector(ITypeInfoStore typeInfoStore)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _typeInfoStore = typeInfoStore;
    }

    public string ColorHex => _typeInfoStore.GetTypeInfoOrBlank(Output.ValueType!).HexColour;

    public IValueOutput Output { get; private set; }

    public Action? PreEvaluateAction => Output.PreEvaluateAction;

    public void Init(IValueOutput nodeOutput) => Output = nodeOutput;

    public void OnDisconnectedFrom(IInputConnector connector)
    {
    }

    public bool TryConnectTo(IInputConnector connector)
    {
        return connector is IInputConnector<IValueInput> valConnector && valConnector.Input.TrySetValueProvider(Output);
    }

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        if (executionFlags.HasValueFlag())
        {
            return PassUpdateOption.AlwaysPasses;
        }

        return PassUpdateOption.NeverPasses;
    }
}
