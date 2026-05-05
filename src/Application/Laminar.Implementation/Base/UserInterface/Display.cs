using System;
using System.ComponentModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

internal class Display(IDisplayValue displayValue, IUserInterfaceProvider userInterfaceProvider) : IDisplay
{
    private object? _lastDisplayValue;
    private IUserInterfaceDefinition? _interfaceDefinition;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<LaminarExecutionContext>? ExecutionStarted
    {
        add => DisplayValue.ExecutionStarted += value;
        remove => DisplayValue.ExecutionStarted -= value;
    }

    public IDisplayValue DisplayValue { get; } = displayValue;

    public object? Interface
    {
        get
        {
            if (_interfaceDefinition is null)
            {
                Refresh();
            }

            return userInterfaceProvider.GetUserInterface(_interfaceDefinition);
        }
    }

    public void Refresh()
    {
        if (!Equals(_lastDisplayValue, DisplayValue.Value))
        {
            DisplayValue.Refresh();
            _lastDisplayValue = DisplayValue.Value;
        }

        var newDefinition = DisplayValue.InterfaceDefinition;
        if (_interfaceDefinition == newDefinition) return;
        
        _interfaceDefinition = newDefinition;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interface)));
    }
}
