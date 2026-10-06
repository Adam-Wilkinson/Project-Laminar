using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Laminar.Domain.Extensions;

namespace Laminar.Avalonia.Controls;

public partial class EditableLabel : UserControl
{
    public static readonly StyledProperty<bool> IsBeingEditedProperty = AvaloniaProperty.Register<EditableLabel, bool>(nameof(IsBeingEdited));
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<EditableLabel, string>(nameof(Text));
    public static readonly StyledProperty<char[]> DisallowedCharsProperty = AvaloniaProperty.Register<EditableLabel, char[]>(nameof(DisallowedChars));
    public static readonly StyledProperty<string?> DisplayStringFormatProperty = AvaloniaProperty.Register<EditableLabel, string?>(nameof(DisplayStringFormat));
    
    private static readonly TimeSpan InvalidCharHintDuration = new(0, 0, 5);

    private readonly StackPanel _invalidCharHint = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        Spacing = 10,
    };
    private readonly Flyout _invalidCharHintFlyout;

    private DateTime _furthestCharHintFlyoutCloseTime = DateTime.Now;

    static EditableLabel()
    {
        IsBeingEditedProperty.Changed.AddClassHandler<EditableLabel>((label, args) => label.IsBeingEditedChanged(args));
        DisallowedCharsProperty.Changed.AddClassHandler<EditableLabel>((label, _) => label.DisallowedCharsChanged());
        DisplayStringFormatProperty.Changed.AddClassHandler<EditableLabel>((label, _) => label.DisplayStringFormatChanged());
    }
    
    public EditableLabel()
    {
        InitializeComponent();

        _invalidCharHintFlyout = new Flyout
        {
            Content = new Panel
            {
                Children =
                {
                    _invalidCharHint,
                }
            },
            ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
        };
        
        Display.DoubleTapped += (_, _) => IsBeingEdited = true;
        DisplayStringFormatChanged();
        
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

    public string? DisplayStringFormat
    {
        get => GetValue(DisplayStringFormatProperty);
        set => SetValue(DisplayStringFormatProperty, value);
    }

    private void DisplayStringFormatChanged()
    {
        Display[!TextBlock.TextProperty] = DisplayStringFormat is null
            ? this[!TextProperty]
            : CompiledBinding.Create<EditableLabel, string>(el => el.Text, source: this,
                converter: new FuncValueConverter<string, string>(s => string.Format(DisplayStringFormat, s)));
    }

    private void IsBeingEditedChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.GetNewValue<bool>())
        {
            Editor.Text = Text;
            Display.IsHitTestVisible = false;
            Display.Opacity = 0;
            Editor.IsHitTestVisible = true;
            Editor.Opacity = 1;
            Editor.SelectAll();
            Editor.Focus();
        }
        else
        {
            Display.IsHitTestVisible = true;
            Display.Opacity = 1;
            Editor.IsHitTestVisible = false;
            Editor.Opacity = 0;
            TopLevel.GetTopLevel(this)?.FocusManager.FindNextElement(NavigationDirection.Down)?.Focus();
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
                _ = Dispatcher.UIThread.InvokeAsync(OnInvalidTextEntry);
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
        if (!e.Text.ContainsAny(DisallowedChars)) return;
        
        Dispatcher.UIThread.InvokeAsync(OnInvalidTextEntry);
        e.Handled = true;
    }

    private async Task OnInvalidTextEntry()
    {
        if (!_invalidCharHintFlyout.IsOpen)
        {
            _invalidCharHintFlyout.ShowAt(this);
        }
        
        var myCloseTime = DateTime.Now + InvalidCharHintDuration;
        _furthestCharHintFlyoutCloseTime = myCloseTime;
        await Task.Delay(InvalidCharHintDuration);

        if (myCloseTime >= _furthestCharHintFlyoutCloseTime)
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
    
    private void DisallowedCharsChanged()
    {
        var disallowedCharsInline = DisallowedChars
            .Where(IsUserFriendlyChar)
            .Select(ch => new Run(ch.ToString()) { Classes = { "Emphasis" }})
            .InsertInBetween(new Run("', '"));

        _invalidCharHint.Children.Clear();
        _invalidCharHint.Children.Add(new TextBlock 
        { 
            Text = "Invalid character!", 
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        InlineCollection secondLineInlines =
        [
            new Run("Invalid characters are '"),
            .. disallowedCharsInline,
            new Run("'.")
        ];

        var secondLine = new TextBlock
        {
            Inlines = secondLineInlines
        };
        
        secondLine.Classes.Add("b2");
        _invalidCharHint.Children.Add(secondLine);
    }
}