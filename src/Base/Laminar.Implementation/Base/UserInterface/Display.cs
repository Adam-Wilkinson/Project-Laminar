using System.ComponentModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

internal class Display : IDisplay
{
    readonly IUserInterfaceProvider _userInterfaceProvider;

    object? _lastDisplayValue;
    IUserInterfaceDefinition? _interfaceDefinition;

    public Display(IDisplayValue displayValue, IUserInterfaceProvider userInterfaceProvider)
    {
        DisplayValue = displayValue;
        _userInterfaceProvider = userInterfaceProvider;
        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisplayValue DisplayValue { get; }

    public object? Interface
    {
        get
        {
            if (_interfaceDefinition is null)
            {
                Refresh();
            }

            return _userInterfaceProvider.GetUserInterface(_interfaceDefinition);
        }
    }

    public void Refresh()
    {
        if (!Equals(_lastDisplayValue, DisplayValue.Value))
        {
            DisplayValue.Refresh();
            _lastDisplayValue = DisplayValue.Value;
        }

        IUserInterfaceDefinition newDefinition = DisplayValue.InterfaceDefinition;
        if (_interfaceDefinition != newDefinition)
        {
            _interfaceDefinition = newDefinition;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interface)));
    }
}
