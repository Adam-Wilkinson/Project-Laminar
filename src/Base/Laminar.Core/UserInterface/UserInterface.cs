using System;
using System.ComponentModel;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Core.UserInterface;

internal class UserInterface : IUserInterface
{
    readonly IValueInfo _valueInfo;
    readonly IUserInterfaceProvider _userInterfaceProvider;
    readonly string _frontendKey;

    IUserInterfaceDefinition? _interfaceDefinition;

    public UserInterface(IValueInfo valueInfo, string frontendKey, IUserInterfaceProvider interfaceProvider)
    {
        _valueInfo = valueInfo;
        _userInterfaceProvider = interfaceProvider;
        _frontendKey = frontendKey;
        DisplayValue = new DisplayValue(valueInfo);
        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisplayValue DisplayValue { get; }

    public object Interface { get; private set; }

    public void Refresh()
    {
        (DisplayValue as DisplayValue)?.CheckForValueChange();
        IUserInterfaceDefinition newDefinition = GetCurrentDefinition();
        if (_interfaceDefinition != newDefinition)
        {
            _interfaceDefinition = newDefinition;
            (DisplayValue as DisplayValue).InterfaceDefinition = _interfaceDefinition;
            Interface = GetInterface();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interface)));
        }
    }

    private IUserInterfaceDefinition GetCurrentDefinition()
    {
        if (_valueInfo.IsUserEditable && _valueInfo.Editor is not null && _userInterfaceProvider.InterfaceImplemented(_valueInfo.Editor, _frontendKey))
        {
            return _valueInfo.Editor;
        }

        if (!_valueInfo.IsUserEditable && _valueInfo.Viewer is not null && _userInterfaceProvider.InterfaceImplemented(_valueInfo.Viewer, _frontendKey))
        {
            return _valueInfo.Viewer;
        }

        return _userInterfaceProvider.GetDefaultDefinition(_valueInfo.ValueType, _valueInfo.IsUserEditable);
    }

    private object GetInterface()
    {
        return _userInterfaceProvider.GetUserInterface(_interfaceDefinition, _frontendKey);
    }
}