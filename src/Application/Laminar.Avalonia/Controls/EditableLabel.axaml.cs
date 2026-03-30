using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Laminar.Avalonia.Shapes;

namespace Laminar.Avalonia.Controls;

public partial class EditableLabel : UserControl
{
    public static readonly StyledProperty<bool> IsBeingEditedProperty = AvaloniaProperty.Register<EditableLabel, bool>(nameof(IsBeingEdited));
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<EditableLabel, string>(nameof(Text));
    public static readonly StyledProperty<char[]> DisallowedCharsProperty = AvaloniaProperty.Register<EditableLabel, char[]>(nameof(DisallowedChars));

    private static readonly TimeSpan InvalidCharHintDuration = new(0, 0, 3);

    private readonly TextBlock _invalidCharHintText = new();
    private readonly Flyout _invalidCharHintFlyout;

    private DateTime _furthestCloseTime = DateTime.Now;
    
    static EditableLabel()
    {
        IsBeingEditedProperty.Changed.AddClassHandler<EditableLabel>((label, args) => label.IsBeingEditedChanged(args));
    }

    public EditableLabel()
    {
        InitializeComponent();

        _invalidCharHintFlyout = new()
        {
            Content = new Panel
            {
                Children =
                {
                    // new PopupBackground()
                    // {
                    //     TeardropSize = 12,
                    //     HorizontalAlignment = HorizontalAlignment.Stretch, 
                    //     VerticalAlignment = VerticalAlignment.Stretch,
                    //     Fill = Brushes.Black
                    // },
                    _invalidCharHintText,
                }
            },
            ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
        };
        
        Display.DoubleTapped += (_, _) => IsBeingEdited = true;
        Display[!TextBlock.TextProperty] = this[!TextProperty];
        
        Editor.KeyDown += Entry_KeyDown;
        Editor.AddHandler(TextInputEvent, Editor_TextInput, RoutingStrategies.Tunnel);
        Editor.PastingFromClipboard += EditorOnPastingFromClipboard;

        Editor.AttachedToVisualTree += (_, _) =>
        {
            if (IsBeingEdited)
            {
                Editor.Focus();
            }
        };

        Editor.LostFocus += (_, _) =>
        {
            IsBeingEdited = false;
        };
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public bool IsBeingEdited
    {
        get => GetValue(IsBeingEditedProperty);
        set => SetValue(IsBeingEditedProperty, value);
    }

    public char[] DisallowedChars
    {
        get => GetValue(DisallowedCharsProperty);
        set => SetValue(DisallowedCharsProperty, value);
    }

    private void IsBeingEditedChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.GetNewValue<bool>())
        {
            Editor.Text = Text;
            Display.IsVisible = false;
            Editor.IsVisible = true;
            Editor.SelectAll();
            Editor.Focus();
        }
        else
        {
            Display.IsVisible = true;
            Editor.IsVisible = false;
        }
    }

    private void Entry_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                Text = Editor.Text ?? string.Empty;
                IsBeingEdited = false;
                e.Handled = true;
                break;
            case Key.Escape:
                IsBeingEdited = false;
                e.Handled = true;
                break;
        }
    }
    
    private async void EditorOnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not { Clipboard: { } clipboard }) return;

        e.Handled = true;

        try
        {
            var text = await clipboard.TryGetTextAsync();
            if (text.ContainsAny(DisallowedChars))
            {
                Dispatcher.UIThread.InvokeAsync(OnInvalidTextEntry);
            }
            else
            {
                Editor.Text += text;
            }
        }
        catch (TimeoutException)
        {
            
        }
    }
    
    private void Editor_TextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Text.ContainsAny(DisallowedChars))
        {
            Dispatcher.UIThread.InvokeAsync(OnInvalidTextEntry);
            e.Handled = true;
        }
    }

    private async Task OnInvalidTextEntry()
    {
        _invalidCharHintText.Text = "Invalid character input \n Invalid characters are: "
                                    + string.Join("', '", DisallowedChars.Where(IsUserFriendlyChar));

        if (!_invalidCharHintFlyout.IsOpen)
        {
            _invalidCharHintFlyout.ShowAt(this);
        }
        
        var myCloseTime = DateTime.Now + InvalidCharHintDuration;
        _furthestCloseTime = myCloseTime;
        await Task.Delay(InvalidCharHintDuration);

        if (myCloseTime >= _furthestCloseTime)
        {
            _invalidCharHintFlyout.Hide();
        }
    }

    private static bool IsUserFriendlyChar(char c)
    {
        return char.GetUnicodeCategory(c) != UnicodeCategory.Control &&
               char.GetUnicodeCategory(c) != UnicodeCategory.OtherNotAssigned && 
               char.GetUnicodeCategory(c) != UnicodeCategory.Format &&
               char.GetUnicodeCategory(c) != UnicodeCategory.PrivateUse &&
               char.GetUnicodeCategory(c) != UnicodeCategory.Surrogate;
    }
}