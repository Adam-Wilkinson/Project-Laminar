using System.ComponentModel;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Core.UserInterface;
internal class Display : IDisplay
{
    readonly IValueInfo _valueInfo;
    readonly IUserInterfaceProvider _userInterfaceProvider;

    IUserInterfaceDefinition? _interfaceDefinition;
    object? _interface;

    public Display(IValueInfo valueInfo, IUserInterfaceProvider userInterfaceProvider)
    {
        _valueInfo = valueInfo;
        _userInterfaceProvider = userInterfaceProvider;
        Value = new DisplayValue(valueInfo);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisplayValue Value { get; }

    public object Interface => _interface ?? RefreshAndReturnInterface();

    public void Refresh() => RefreshAndReturnInterface();

    private object RefreshAndReturnInterface()
    {
        (Value as DisplayValue)?.CheckForValueChange();
        IUserInterfaceDefinition newDefinition = _userInterfaceProvider.FindDefinitionForValueInfo(_valueInfo);
        if (_interfaceDefinition != newDefinition)
        {
            _interfaceDefinition = newDefinition;
            (Value as DisplayValue).InterfaceDefinition = _interfaceDefinition;
            _interface = GetInterface();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interface)));
        }

        return _interface;
    }

    private object GetInterface()
    {
        return _userInterfaceProvider.GetUserInterface(_interfaceDefinition);
    }
}
