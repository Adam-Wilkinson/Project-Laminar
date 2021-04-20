using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_Avalonia.Models;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using System.Diagnostics;

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

        public void OpenScriptEditor(INodeTree script)
        {
            (DataContext as MainWindowViewModel).ShowScriptEditor(script);
        }

        public void ShowAllScripts()
        {
            (DataContext as MainWindowViewModel).ShowAllScripts();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
