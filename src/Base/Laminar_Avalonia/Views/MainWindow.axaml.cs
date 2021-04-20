using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_Avalonia.Models;

namespace Laminar_Avalonia.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
#if DEBUG
            // this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
