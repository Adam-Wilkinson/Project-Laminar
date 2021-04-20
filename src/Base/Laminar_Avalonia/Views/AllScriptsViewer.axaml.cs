using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar_Avalonia.Views
{
    public class AllScriptsViewer : UserControl
    {
        public AllScriptsViewer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
