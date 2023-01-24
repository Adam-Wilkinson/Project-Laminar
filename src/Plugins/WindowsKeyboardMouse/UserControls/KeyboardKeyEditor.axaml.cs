using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WindowsKeyboardMouse.Primitives;

namespace WindowsKeyboardMouse.UserControls;

public class KeyboardKeyEditor : UserControl
{
    private static readonly Key[] ModifierKeys = new[] { Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.RightAlt, Key.RightCtrl, Key.RightShift };

    private readonly TextBlock _mainTextBlock;
    private KeyboardKey _keyValue;
    // private ILaminarValue _laminarValue;
    private State _state;

    public KeyboardKeyEditor()
    {
        InitializeComponent();
        _mainTextBlock = this.FindControl<TextBlock>("MainTextBlock");
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        //if (DataContext is ILaminarValue laminarValue)
        //{
        //    if (_laminarValue is not null)
        //    {
        //        _laminarValue.OnChange -= LaminarValue_OnChange;
        //    }

        //    _laminarValue = laminarValue;
        //    _laminarValue.OnChange += LaminarValue_OnChange;
        //    UpdateTextBlock(_laminarValue.Value);
        //}
        base.OnDataContextChanged(e);
    }

    private void LaminarValue_OnChange(object sender, object e)
    {
        // UpdateTextBlock(_laminarValue.Value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_state is State.ChangingKey or State.FindingModifierKey)
        {
            _keyValue = new KeyboardKey(AvaloniaKeyTools.GetVirtualKey(e.Key), (Primitives.KeyModifiers)(int)e.KeyModifiers);
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
        base.OnKeyDown(e);
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
        _mainTextBlock.Text = $">> Press a key <<";
        Focus();
        base.OnPointerPressed(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void UpdateTextBlock(object value)
    {
        //if (_laminarValue.Name is not "" or null)
        //{
        //    _mainTextBlock.Text = $"{_laminarValue.Name}: {value}";
        //}
        //else
        //{
        //    _mainTextBlock.Text = value.ToString();
        //}
    }


    private void FinishFindingKey()
    {
        _state = State.None;
        // _laminarValue.Value = _keyValue;
    }

    private enum State
    {
        None,
        ChangingKey,
        FindingModifierKey,
    }
}
