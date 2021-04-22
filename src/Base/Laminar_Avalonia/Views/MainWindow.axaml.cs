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

        public void DeleteScript(IScriptInstance script)
        {
            App.LaminarInstance.AllScripts.Scripts.Remove(script);
        }

        public void AddScriptInstance(INodeTree script)
        {
            IAdvancedScriptInstance newScript = App.LaminarInstance.Factory.GetImplementation<IAdvancedScriptInstance>();
            newScript.Script = script;
            newScript.Name.Value = script.Name.Value;
            App.LaminarInstance.AllScripts.Scripts.Add(newScript);
        }

        public async Task AddScript()
        {
            string text = await TextPrompt.Show(this, "Script Maker", "Please enter the name of the script");

            if (text is null or "")
            {
                return;
            }

            INodeTree newTree = App.LaminarInstance.Factory.GetImplementation<INodeTree>();
            newTree.Name.Value = text;
            App.LaminarInstance.AllAdvancedScripts.Add(newTree);
            AddScriptInstance(newTree);

            OpenScriptEditor(newTree);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
