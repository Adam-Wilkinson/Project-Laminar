using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Implementation.Base.UserInterface;

internal class DisplayValue : IDisplayValue
{
    private readonly IValueInfo _valueInfo;
    private object _value;

    public DisplayValue(IValueInfo valueInfo)
    {
        _valueInfo = valueInfo;
    }

    public string Name => _valueInfo.Name;

    public object Value
    {
        get => _value;
        set
        {
            _valueInfo.BoxedValue = value;
            _value = _valueInfo.BoxedValue;
        }
    }

    public IUserInterfaceDefinition InterfaceDefinition { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public void CheckForValueChange()
    {
        if (_value != _valueInfo.BoxedValue)
        {
            _value = _valueInfo.BoxedValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}