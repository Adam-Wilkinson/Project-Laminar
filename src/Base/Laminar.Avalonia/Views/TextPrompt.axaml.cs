using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Laminar.Avalonia.Views;

public class TextPrompt : Window
{
    private bool _shouldReturnText = false;

    public TextPrompt()
    {
        InitializeComponent();
        DataContext = this;
        this.FindControl<TextBox>("EnterText").KeyDown += (_, e) =>
        {
            if (e.Key is Key.Enter)
            {
                _shouldReturnText = true;
            }

            if (e.Key is Key.Enter or Key.Escape)
            {
                Close();
            }
        };
    }

    public string EntryBoxText { get; set; }

    public string OutputText => _shouldReturnText ? EntryBoxText : null;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void Ok()
    {
        _shouldReturnText = true;
        Close();
    }

    public void Cancel()
    {
        Close();
    }

    public static Task<string> Show(Window parent, string title, string text)
    {
        TextPrompt textPrompt = new()
        {
            Title = title
        };

        textPrompt.FindControl<TextBlock>("InfoText").Text = text;

        var tcs = new TaskCompletionSource<string>();

        textPrompt.Closed += delegate
        {
            tcs.TrySetResult(textPrompt.OutputText);
        };

        textPrompt.ShowDialog(parent);

        textPrompt.FindControl<TextBox>("EnterText").Focus();

        return tcs.Task;
    }
}
