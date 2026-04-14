using System;
using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.UserData.Settings;

internal class SettingsDisplayValue<T>(string name, T value) : IDisplayValue
{
    public string Name { get; } = name;

    public object? Value { get; set; } = value;

    public IUserInterfaceDefinition? InterfaceDefinition { get; } = null;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<LaminarExecutionContext>? ExecutionStarted { add { } remove { } }

    public void Refresh() => PropertyChanged?.Invoke(this, IDisplayValue.ValueChangedEventArgs);
}