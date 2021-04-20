using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar_Avalonia.Views
{
    public class ScriptEditor : UserControl
    {
        public ScriptEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
