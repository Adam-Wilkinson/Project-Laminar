using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_Core.Scripts;

namespace Laminar_Avalonia.Views
{
    public class AllScriptsViewer : UserControl
    {
        public AllScriptsViewer()
        {
            InitializeComponent();
            IScriptCollection dc = App.LaminarInstance.Factory.GetImplementation<IScriptCollection>();
            dc.AddScript();
            dc.AddScript();
            DataContext = dc;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
