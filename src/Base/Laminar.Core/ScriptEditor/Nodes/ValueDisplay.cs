using System;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Core.ScriptEditor.Nodes;

internal class ValueDisplay : IValueDisplay
{
    private readonly IUserInterfaceFactory _interfaceFactory;
    private string _frontendKey;
    private IUserInterface _interface;

    public ValueDisplay(IValueInfo value, IUserInterfaceFactory interfaceFactory)
    {
        ValueInfo = value;
        _interfaceFactory = interfaceFactory;
    }

    public IValueInfo ValueInfo { get; }

    public IUserInterface this[string frontendKey]
    {
        get
        {
            if (frontendKey == null)
            {
                _frontendKey = null;
                _interface = null;
            }
            else if (_frontendKey is null)
            {
                _frontendKey = frontendKey;
                _interface = _interfaceFactory.CreateUserInterface(ValueInfo, frontendKey);
            }
            else if (_frontendKey != frontendKey)
            {
                throw new Exception("Value display can only have one frontend type");
            }

            return _interface;
        }
    }

    public bool CopyValueTo(IValueDisplay value)
    {
        value[_frontendKey].DisplayValue.Value = this[_frontendKey].DisplayValue.Value;
        return true;
    }

    public void Refresh()
    {
        _interface?.Refresh();
    }
}