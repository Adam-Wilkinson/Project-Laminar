using System;
using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Notification;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeIO;

public class ValueInput<T> : IValueInput<T>, INotificationClient
{
    readonly LaminarExecutionContext _contextCache;
    readonly DisplayValue<T> _displayValue;

    internal ValueInput(IUserInterfaceProvider uiProvider, ITypeInfoStore typeInfoStore, string name, T defaultValue)
    {
        InterfaceDefinition = new ValueInterfaceDefinition<T>(typeInfoStore, uiProvider) { IsUserEditable = true };

        _displayValue = new(this, InterfaceDefinition, defaultValue ) { Name = name };

        Connector = new ValueInputConnector<T>(typeInfoStore) { Input = this };

        _contextCache = new LaminarExecutionContext
        {
            ExecutionFlags = ValueExecutionFlag.Value,
            ExecutionSource = null,
        };
    }

    public T Value
    {
        get => _displayValue.TypedValue;
        set => _displayValue.TypedValue = value;
    }

    public IInputConnector Connector { get; }

    public virtual Action? PreEvaluateAction { get; set; }

    public IValueInterfaceDefinition InterfaceDefinition { get; }

    public IDisplayValue DisplayValue => _displayValue;

    public event EventHandler<LaminarExecutionContext>? ExecutionStarted;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetValueProvider(IValueProvider<T>? provider)
    {
        _displayValue.ValueProvider = provider;
        InterfaceDefinition.IsUserEditable = _displayValue.ValueProvider is null;
        FireValueChange();
    }

    public void TriggerNotification()
    {
        FireValueChange();
    }

    protected void FireValueChange() => ExecutionStarted?.Invoke(this, _contextCache);
}