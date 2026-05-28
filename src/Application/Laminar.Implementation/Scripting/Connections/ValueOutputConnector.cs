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
    
    public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }

    public bool AcceptsConnections => true;

    public string ColorHex => typeInfoStore.GetTypeInfoOrBlank(typeof(T)).HexColor;

    public IValueOutput<T> Output { get; } = output;

    public Action? PreEvaluateAction => Output.PreEvaluateAction;
    
    public void OnConnectionEstablished()
    {
    }

    public void OnConnectionSevered()
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
        if (!executionFlags.HasValueFlag) return PassUpdateOption.NeverPasses;
        if (Output.AlwaysPassUpdate) return PassUpdateOption.AlwaysPasses;
        if (EqualityComparer<T>.Default.Equals(Output.Value, _valueAtLastUpdate)) return PassUpdateOption.CurrentlyDoesNotPass;
        
        _valueAtLastUpdate = Output.Value;
        return PassUpdateOption.CurrentlyPasses;
    }
}
