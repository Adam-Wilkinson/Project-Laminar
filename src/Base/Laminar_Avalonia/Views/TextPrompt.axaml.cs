using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Laminar_Avalonia.Views
{
    public class TextPrompt : Window
    {
        public TextPrompt()
        {
            InitializeComponent();
#if DEBUG
            // this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<string> Show(Window parent, string title, string text)
        {
            TextPrompt textPrompt = new()
            {
                Title = title
            };
            textPrompt.FindControl<TextBlock>("InfoText").Text = text;

            TextBox entrybox = textPrompt.FindControl<TextBox>("EnterText");

            var tcs = new TaskCompletionSource<string>();

            entrybox.KeyDown += (_, e) =>
            {
                if (e.Key is Avalonia.Input.Key.Enter or Avalonia.Input.Key.Escape)
                {
                    textPrompt.Close();
                }
            };

            textPrompt.FindControl<Button>("OkButton").PointerPressed += delegate { textPrompt.Close(); };

            textPrompt.Closed += delegate { tcs.TrySetResult(entrybox.Text); };

            if (parent is not null)
            {
                textPrompt.ShowDialog(parent);
            }
            else
            {
                textPrompt.Show();
            }

            entrybox.Focus();

            return tcs.Task;
        }
    }
}
