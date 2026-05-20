using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain.Notification;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

public class ValueOutput<T> : IValueOutput<T>, INotificationClient where T : notnull
{
    readonly LaminarExecutionContext _contextCache;
    readonly DisplayValue<T> _displayValue;

    public ValueOutput(IUserInterfaceProvider uiProvider, ITypeInfoStore typeInfoStore, T initialValue, string name, ISourcedInterfaceData<T> interfaceData)
    {
        InterfaceDefinition = new ValueInterfaceDefinition<T>(typeInfoStore, uiProvider);
        InterfaceData = interfaceData;
        
        _displayValue = new DisplayValue<T>(this, InterfaceDefinition, initialValue) { Name = name };

        Connector = new ValueOutputConnector<T>(typeInfoStore, this);
        
        Name = name;
        Value = initialValue;

        InterfaceData.ExecutionStarted += OnInterfaceValueChanged;
        
        _contextCache = new LaminarExecutionContext
        {
            ExecutionFlags = ValueExecutionFlag.Value,
            ExecutionSource = Connector,
        };
    }

    public string Name { get; }

    public ISourcedInterfaceData<T> InterfaceData { get; }

    public T Value
    {
        get => InterfaceData.Value;
        set => InterfaceData.QuietSetValue(value);
    }
    
    public Action? PreEvaluateAction => null;

    public IOutputConnector Connector { get; }

    public bool AlwaysPassUpdate { get; init; }

    public IDisplayValue DisplayValue => _displayValue;

    public IValueInterfaceDefinition InterfaceDefinition { get; }

    public event EventHandler<LaminarExecutionContext>? ExecutionStarted;

    public void StartExecution()
    {
        FireValueChange();
    }

    public void TriggerNotification()
    {
        FireValueChange();
    }

    protected void FireValueChange() => ExecutionStarted?.Invoke(this, _contextCache);

    private void OnInterfaceValueChanged(object? _, LaminarExecutionContext args)
    {
        ExecutionStarted?.Invoke(this, args);
    }
}