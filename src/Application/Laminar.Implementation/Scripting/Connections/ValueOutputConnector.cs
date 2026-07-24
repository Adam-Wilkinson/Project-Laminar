using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;

namespace Laminar.Implementation.Scripting.Connections;

internal class ValueOutputConnector<T>(ITypeInfoStore typeInfoStore, IValueOutput<T> output)
    : IOutputConnector<IValueOutput<T>> where T : notnull
{
    private T _valueAtLastUpdate = output.Value;
    private int _connectionCount;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public IValueOutput<T> Output { get; } = output;

    public Action? PreEvaluateAction => Output.PreEvaluateAction;

    public ConnectorFlags Flags { get; private set; } = ConnectorFlags.AcceptsConnections;

    public void OnConnectedTo(IInputConnector input)
    {
        if (input is IInputConnector<IValueInput<T>> inputConnector)
        {
            inputConnector.Input.SetValueProvider(Output);
        }
        
        Flags = ConnectorFlags.AcceptsConnections | ConnectorFlags.HasConnections;
        _connectionCount++;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flags)));
    }

    public void OnDisconnectedFrom(IInputConnector _)
    {
        _connectionCount--;
        Flags = ConnectorFlags.AcceptsConnections;
        if (_connectionCount > 0)
            Flags |= ConnectorFlags.HasConnections;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flags)));
    }

    public bool CanConnectTo(IInputConnector connector)
        => connector is IInputConnector<IValueInput<T>>;

    public PassUpdateOption PassUpdate(ExecutionFlags executionFlags)
    {
        if (!executionFlags.HasValueFlag) return PassUpdateOption.NeverPasses;
        if (Output.AlwaysPassUpdate) return PassUpdateOption.AlwaysPasses;
        if (EqualityComparer<T>.Default.Equals(Output.Value, _valueAtLastUpdate)) return PassUpdateOption.CurrentlyDoesNotPass;
        
        _valueAtLastUpdate = Output.Value;
        return PassUpdateOption.CurrentlyPasses;
    }

    public override string ToString() => $"Value Output '{Output.InterfaceData.Name}' (Value: {Output.Value})";
}
