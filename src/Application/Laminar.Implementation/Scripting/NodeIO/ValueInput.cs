using Laminar.Contracts.Base;
using Laminar.Domain.Notification;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeIO;

internal sealed class ValueInput<T> : IValueInput<T> where T : notnull
{
    internal ValueInput(ITypeInfoStore typeInfoStore)
    {
        Connector = new ValueInputConnector<T>(typeInfoStore) { Input = this };
    }

    public required ISourcedInterfaceData<T> InterfaceData { get; init; }

    public T Value
    {
        get => InterfaceData.Value;
        set => InterfaceData.Value = value;
    }
    
    public IInputConnector Connector { get; }

    public Action? PreEvaluateAction { get; set; }

    public event EventHandler<LaminarExecutionContext>? ExecutionStarted
    {
        add => InterfaceData.ExecutionStarted += value;
        remove => InterfaceData.ExecutionStarted -= value;
    }
    
    public void SetValueProvider(IValueProvider<T>? provider)
    {
        InterfaceData.ValueProvider = provider;
    }
}