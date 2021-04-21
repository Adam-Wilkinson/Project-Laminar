using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_Avalonia.Models;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Scripts;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public void DeleteScript(IScript script)
        {
            App.LaminarInstance.AllScripts.Scripts.Remove(script);
        }

        public async Task AddScript()
        {
            string text = await TextPrompt.Show(this, "Script Maker", "Please enter the name of the script");

            if (text is null or "")
            {
                return;
            }

            IScript newScript = App.LaminarInstance.Factory.GetImplementation<INodeTree>();
            newScript.Name.Value = text;
            App.LaminarInstance.AllScripts.Scripts.Add(newScript);

            if (newScript is INodeTree nodeTree)
            {
                OpenScriptEditor(nodeTree);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
