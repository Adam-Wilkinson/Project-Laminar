using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueInputConnector<T>(ITypeInfoStore typeInfoStore) : IInputConnector<IValueInput<T>> where T : notnull
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool AcceptsConnections { get; set; } = true;

    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public required IValueInput<T> Input { get; init; }

    public Action? PreEvaluateAction => Input.PreEvaluateAction;

    public void OnConnectionEstablished()
    {
        AcceptsConnections = false;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptsConnections)));
    }

    public void OnConnectionSevered()
    {
        Input.SetValueProvider(null);
        AcceptsConnections = true;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptsConnections)));
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
