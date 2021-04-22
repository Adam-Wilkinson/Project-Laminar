using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_Core;
using Laminar_Core.Scripts;

namespace Laminar_Avalonia.Views
{
    public class AllScriptsViewer : UserControl
    {
        public AllScriptsViewer()
        {
            InitializeComponent();
            Instance dc = App.LaminarInstance;
            DataContext = dc;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
