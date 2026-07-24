using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector<T>(ITypeInfoStore typeInfoStore) : IInputConnector<IValueInput<T>> where T : notnull
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ConnectorFlags Flags { get; private set; } = ConnectorFlags.AcceptsConnections;

    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public required IValueInput<T> Input { get; init; }

    public Action? PreEvaluateAction => Input.PreEvaluateAction;

    public void OnConnectedTo(IOutputConnector output)
    {
        if (output is IOutputConnector<IValueOutput<T>> outputConnector)
        {
            Input.SetValueProvider(outputConnector.Output);
        }
        
        Flags = ConnectorFlags.HasConnections | ConnectorFlags.ConnectionsSaturated;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flags)));
    }

    public void OnDisconnectedFrom(IOutputConnector _)
    {
        Input.SetValueProvider(null);
        Flags = ConnectorFlags.AcceptsConnections;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flags)));
    }

    public bool CanConnectTo(IOutputConnector connector)
        => connector is IOutputConnector<IValueOutput<T>>;

    public override string ToString() => $"Value Input '{Input.InterfaceData.Name}' (Value: {Input.Value})";
}
