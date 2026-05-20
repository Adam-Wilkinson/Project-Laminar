using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector<T>(ITypeInfoStore typeInfoStore) : IInputConnector<IValueInput<T>> where T : notnull
{
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public required IValueInput<T> Input { get; init; }

    public Action? PreEvaluateAction => Input.PreEvaluateAction;

    public void OnDisconnectedFrom(IOutputConnector connector)
    {
        Input.SetValueProvider(null);
    }

    public bool CanConnectTo(IOutputConnector connector)
        => connector is IOutputConnector<IValueOutput<T>>;

    public bool TryConnectTo(IOutputConnector connector)
    {
        if (connector is not IOutputConnector<IValueOutput<T>> outputConnector) return false;
        
        Input.SetValueProvider(outputConnector.Output);
        return true;

    }
}
