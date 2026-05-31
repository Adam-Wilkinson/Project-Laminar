using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector<T>(ITypeInfoStore typeInfoStore) : IInputConnector<IValueInput<T>> where T : notnull
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ConnectorStatus Status { get; private set; } = ConnectorStatus.AcceptsConnections;

    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public required IValueInput<T> Input { get; init; }

    public Action? PreEvaluateAction => Input.PreEvaluateAction;

    public void OnConnectionEstablished()
    {
        Status = ConnectorStatus.ConnectionsSaturated;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
    }

    public void OnConnectionSevered()
    {
        Input.SetValueProvider(null);
        Status = ConnectorStatus.AcceptsConnections;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
    }

    public bool CanConnectTo(IOutputConnector connector)
        => connector is IOutputConnector<IValueOutput<T>>;

    public bool TryConnectTo(IOutputConnector connector)
    {
        if (connector is not IOutputConnector<IValueOutput<T>> outputConnector) return false;
        
        Input.SetValueProvider(outputConnector.Output);
        return true;
    }

    public override string ToString() => $"Value Input '{Input.InterfaceData.Name}' (Value: {Input.Value})";
}
