using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Primitives;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeIO;

public class ValueOutput<T> : IValueOutput<T>, INotificationClient
{
    readonly LaminarExecutionContext _contextCache;
    readonly DisplayValue<T> _displayValue;

    public ValueOutput(IUserInterfaceProvider uiProvider, ITypeInfoStore typeInfoStore, string name, T initialValue)
    {
        InterfaceDefinition = new ValueInterfaceDefinition<T>(typeInfoStore, uiProvider);

        _displayValue = new DisplayValue<T>(this, InterfaceDefinition, initialValue) { Name = name };

        Connector = new ValueOutputConnector<T>(typeInfoStore, this);

        _contextCache = new LaminarExecutionContext
        {
            ExecutionFlags = ValueExecutionFlag.Value,
            ExecutionSource = Connector,
        };
    }

    public T Value
    {
        get => _displayValue.TypedValue;
        set => _displayValue.TypedValue = value;
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
}