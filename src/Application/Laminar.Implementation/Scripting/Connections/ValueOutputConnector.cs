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
    private int _connectionCount = 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public IValueOutput<T> Output { get; } = output;

    public Action? PreEvaluateAction => Output.PreEvaluateAction;

    public ConnectorFlags Flags { get; private set; } = ConnectorFlags.AcceptsConnections;

    public void OnConnectionEstablished()
    {
        Flags = ConnectorFlags.AcceptsConnections | ConnectorFlags.HasConnections;
        _connectionCount++;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flags)));
    }

    public void OnConnectionSevered()
    {
        _connectionCount--;
        if (_connectionCount == 0)
        {
            Flags = ConnectorFlags.AcceptsConnections;   
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flags)));
    }

    public bool CouldConnectTo(IInputConnector connector)
        => connector is IInputConnector<IValueInput<T>>; 
    
    public bool TryConnectTo(IInputConnector connector)
    {
        if (connector is not IInputConnector<IValueInput<T>> inputConnector) return false;
        inputConnector.Input.SetValueProvider(Output);
        return true;
    }

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
