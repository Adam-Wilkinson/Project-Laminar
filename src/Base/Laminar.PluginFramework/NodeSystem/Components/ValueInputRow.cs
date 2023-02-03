using System;
using System.Collections;
using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ValueInputRow<T> : SingleItemNodeComponent
{
    readonly ValueInput<T> _valueInput;

    public ValueInputRow(string name, T defaultValue)
    {
        _valueInput = new(name, defaultValue);
        ChildComponent = Component.Row(_valueInput, _valueInput, null);
    }

    public T Value => _valueInput.Value;

    public static implicit operator T(ValueInputRow<T> valueInputRow)
    {
        return valueInputRow.Value;
    }

    public IUserInterfaceDefinition? Editor
    {
        get => _valueInput.Editor;
        set => _valueInput.Editor = value;
    }

    public IUserInterfaceDefinition? Display
    {
        get => _valueInput.Editor;
        set => _valueInput.Editor = value;
    }
}
