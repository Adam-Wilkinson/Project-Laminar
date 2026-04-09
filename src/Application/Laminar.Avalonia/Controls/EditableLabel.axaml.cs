using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Laminar.Domain.Extensions;

namespace Laminar.Avalonia.Controls;

public partial class EditableLabel : UserControl
{
    public static readonly StyledProperty<bool> IsBeingEditedProperty = AvaloniaProperty.Register<EditableLabel, bool>(nameof(IsBeingEdited));
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<EditableLabel, string>(nameof(Text));
    public static readonly StyledProperty<char[]> DisallowedCharsProperty = AvaloniaProperty.Register<EditableLabel, char[]>(nameof(DisallowedChars));

    private static readonly TimeSpan InvalidCharHintDuration = new(0, 0, 5);
    private static readonly TimeSpan EditingStartedCooldown = new(0, 0, 0, 0, 100);

    private readonly StackPanel _invalidCharHint = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        Spacing = 10,
    };
    private readonly Flyout _invalidCharHintFlyout;

    private DateTime _furthestCharHintFlyoutCloseTime = DateTime.Now;
    private DateTime _editingStartedTime = DateTime.Now;
    
    static EditableLabel()
    {
        IsBeingEditedProperty.Changed.AddClassHandler<EditableLabel>((label, args) => label.IsBeingEditedChanged(args));
        DisallowedCharsProperty.Changed.AddClassHandler<EditableLabel>((label, args) => label.DisallowedCharsChanged(args));
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
                    _invalidCharHint,
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
            if (DateTime.Now - _editingStartedTime < EditingStartedCooldown)
            {
                Editor.SelectAll();
                Editor.Focus();
                return;
            }
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
            _editingStartedTime = DateTime.Now;
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


    private void DisallowedCharsChanged(AvaloniaPropertyChangedEventArgs args)
    {
        IEnumerable<Run> disallowedCharsInline = DisallowedChars
            .Where(IsUserFriendlyChar)
            .Select(ch =>
            {
                var output = new Run(ch.ToString());
                output.Classes.Add("Emphasis");
                return output;
            }).InsertInBetween(new Run("', '"));

        _invalidCharHint.Children.Clear();
        _invalidCharHint.Children.Add(new TextBlock 
        { 
            Text = "Invalid character!", 
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        InlineCollection secondLineInlines = [new Run("Invalid characters are '")];
        secondLineInlines.AddRange(disallowedCharsInline);
        secondLineInlines.Add(new Run("'."));

        var secondLine = new TextBlock
        {
            Inlines = secondLineInlines
        };
        secondLine.Classes.Add("b2");
        _invalidCharHint.Children.Add(secondLine);
    }
}