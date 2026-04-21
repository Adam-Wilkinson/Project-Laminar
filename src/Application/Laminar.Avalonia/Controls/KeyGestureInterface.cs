using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace Laminar.Avalonia.Controls;

[PseudoClasses(":isBeingEdited")]
public class KeyGestureInterface : TemplatedControl
{
    public static readonly StyledProperty<string?> FormatStringProperty =
        AvaloniaProperty.Register<KeyGestureInterface, string?>(nameof(FormatString));

    public static readonly StyledProperty<KeyGestureInterfaceState> StateProperty =
        AvaloniaProperty.Register<KeyGestureInterface, KeyGestureInterfaceState>(nameof(State));

    public static readonly StyledProperty<KeyGesture> GestureProperty =
        AvaloniaProperty.Register<KeyGestureInterface, KeyGesture>(nameof(Gesture), new(Key.None));
        
    public static readonly DirectProperty<KeyGestureInterface, string> TextProperty =
        AvaloniaProperty.RegisterDirect<KeyGestureInterface, string>(nameof(Text), kgi => kgi.Text);
    
    private static readonly KeyGestureFormatInfo FormatInfo = new(new Dictionary<Key, string>
    {
        [Key.LeftCtrl] = "Left Control",
        [Key.RightCtrl] = "Right Control",
        [Key.LeftAlt] = "Left Alt",
        [Key.RightAlt] = "Right Alt",
        [Key.LeftShift] = "Left Shift",
        [Key.RightShift] = "Right Shift",
        [Key.LWin] = "Left " + KeyGestureFormatInfo.GetInstance(null).Meta,
        [Key.RWin] = "Right " + KeyGestureFormatInfo.GetInstance(null).Meta,
    }, KeyGestureFormatInfo.GetInstance(null).Meta, KeyGestureFormatInfo.GetInstance(null).Ctrl, 
        KeyGestureFormatInfo.GetInstance(null).Alt,  KeyGestureFormatInfo.GetInstance(null).Shift);
    
    private KeyGesture _currentGesture = new(Key.None);
    
    public KeyGestureInterface()
    {
        this.GetObservable(GestureProperty).Subscribe(new AnonymousObserver<KeyGesture>(newValue =>
        {
            _currentGesture = newValue;
            RefreshText();
        }));

        this.GetObservable(FormatStringProperty).Subscribe(new AnonymousObserver<string?>(newFormatString =>
        {
            RefreshText();
        }));

        this.GetObservable(StateProperty).Subscribe(new AnonymousObserver<KeyGestureInterfaceState>(newValue =>
        {
            if (newValue is not KeyGestureInterfaceState.None)
            {
                PseudoClasses.Add(":isBeingEdited");
            }
            else
            {
                PseudoClasses.Remove(":isBeingEdited");
            }
        }));
    }
    
    public string Text
    {
        get;
        private set => SetAndRaise(TextProperty, ref field, value);
    } = string.Empty;
    
    public string? FormatString
    {
         get => GetValue(FormatStringProperty);
         set => SetValue(FormatStringProperty, value);
    }

    public KeyGestureInterfaceState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public KeyGesture Gesture
    {
        get => GetValue(GestureProperty);
        set => SetValue(GestureProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (State is KeyGestureInterfaceState.None) return;

        // Sanitize key modifiers, e.g. if left control was most recently pressed, it is the key and not the modifier.
        var keyModifiers = e.KeyModifiers & ~FindModifierKey(e.Key);
        
        _currentGesture = new KeyGesture(e.Key, keyModifiers);
        RefreshText();
        if (FindModifierKey(e.Key) is not KeyModifiers.None)
        {
            State = KeyGestureInterfaceState.FindingModifierKeys;
        }
        else
        {
            FinishFindingKeyGesture();
        }

        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (State is KeyGestureInterfaceState.FindingModifierKeys && FindModifierKey(e.Key) is not KeyModifiers.None)
        {
            FinishFindingKeyGesture();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        State = KeyGestureInterfaceState.ChangingKey;
        Focus();
    }
    
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        State = KeyGestureInterfaceState.None;
    }

    private void FinishFindingKeyGesture()
    {
        State = KeyGestureInterfaceState.None;
        Gesture = _currentGesture;
    }

    private void RefreshText()
    {
        string keyGestureString = _currentGesture.ToString("p", FormatInfo);
        Text = FormatString is null ? keyGestureString : string.Format(FormatString, keyGestureString);
    }

    private static KeyModifiers FindModifierKey(Key key) => key switch
    {
        Key.LeftCtrl or Key.RightCtrl => KeyModifiers.Control,
        Key.LeftAlt or  Key.RightAlt => KeyModifiers.Alt,
        Key.LeftShift or Key.RightShift => KeyModifiers.Shift,
        Key.LWin or Key.RWin => KeyModifiers.Meta,
        _ => KeyModifiers.None,
    };
}

public enum KeyGestureInterfaceState
{
    None,
    ChangingKey,
    FindingModifierKeys
}