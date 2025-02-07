using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Avalonia.Controls;

public partial class KeyGestureEditor : UserControl
{
    private static readonly Key[] ModifierKeys = [ Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.RightAlt, Key.RightCtrl, Key.RightShift ];

    private State _state;
    private KeyGesture? _keyValue;
    private IInterfaceData? _interfaceData;
    
    public KeyGestureEditor()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is IInterfaceData { Value: KeyGesture keyGesture } interfaceData)
        {
            _interfaceData = interfaceData;
            UpdateTextBlock(keyGesture);
        }
        base.OnDataContextChanged(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_state is not (State.ChangingKey or State.FindingModifierKey)) return;
        
        _keyValue = new KeyGesture(e.Key, e.KeyModifiers);
        UpdateTextBlock(_keyValue);
        if (ModifierKeys.Contains(e.Key))
        {
            _state = State.FindingModifierKey;
        }
        else
        {
            FinishFindingKey();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (_state is State.FindingModifierKey && ModifierKeys.Contains(e.Key))
        {
            FinishFindingKey();
        }
        base.OnKeyUp(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _state = State.ChangingKey;
        MainTextBlock.Text = $">> Press a key <<";
        Focus();
        base.OnPointerPressed(e);
    }

    private void UpdateTextBlock(object value)
    {
        if (_interfaceData is null) return;

        MainTextBlock.Text = string.IsNullOrEmpty(_interfaceData.Name) ? value.ToString() : $"{_interfaceData.Name}: {value}";
    }


    private void FinishFindingKey()
    {
        _state = State.None;
        _interfaceData!.Value = _keyValue!;
    }

    private enum State
    {
        None,
        ChangingKey,
        FindingModifierKey,
    }
}