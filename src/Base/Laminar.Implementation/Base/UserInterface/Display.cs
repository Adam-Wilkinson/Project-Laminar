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
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisplayValue DisplayValue { get; }

    public object Interface => RefreshAndReturnInterface();

    public void Refresh() => RefreshAndReturnInterface();

    private object RefreshAndReturnInterface()
    {
        if (DisplayValue.Value != _lastDisplayValue)
        {
            DisplayValue.Refresh();
            _lastDisplayValue = DisplayValue.Value;
        }

        IUserInterfaceDefinition newDefinition = DisplayValue.InterfaceDefinition;
        if (_interfaceDefinition != newDefinition)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interface)));
            _interfaceDefinition = DisplayValue.InterfaceDefinition;
        }

        return _userInterfaceProvider.GetUserInterface(_interfaceDefinition);
    }
}
