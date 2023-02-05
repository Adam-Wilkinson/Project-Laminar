using System;
using System.ComponentModel;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.Settings;

internal class DisplayValue<T> : IDisplayValue
{
    ValueInterfaceDefinition _interfaceDefinition = new()
    {
        ValueType = typeof(T),
        IsUserEditable = true,
    };

    public DisplayValue(
        IUserInterfaceDefinitionFinder uiFinder,
        string name, 
        T value)
    {
        InterfaceDefinition = uiFinder.GetCurrentDefinitionOf(_interfaceDefinition);
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public object? Value { get; set; }

    public IUserInterfaceDefinition InterfaceDefinition { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh() => PropertyChanged?.Invoke(this, IDisplayValue.ValueChangedEventArgs);
}
