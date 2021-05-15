using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Laminar_Avalonia.Models;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Instancing;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Laminar_Avalonia.Views
{
    public class MainWindow : Window
    {
        private bool _needsScriptInstance = false;

        public MainWindow()
        {
            FontFamily = new FontFamily("Lucida Sans");
            InitializeComponent();
            DataContext = new MainWindowViewModel();
#if DEBUG
            // this.AttachDevTools();
#endif
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) == true)
            {
                UseNativeTitleBar();
            }

            Resources["HeaderColour"] = new SolidColorBrush(new Color(255, 19, 19, 35));
        }

        public void OpenEditorOfInstance(IAdvancedScriptInstance scriptInstance)
        {
            OpenScriptEditor(scriptInstance.CompiledScript.OriginalScript);
        }

        public void OpenScriptEditor(IAdvancedScript script)
        {
            (DataContext as MainWindowViewModel).ShowScriptEditor(script);
        }

        public void CloseScriptEditor()
        {
            if (DataContext is MainWindowViewModel mwvm && mwvm.MainControl is ScriptEditor scriptEditor && scriptEditor.DataContext is IAdvancedScript openNodeTree)
            {
                openNodeTree.IsBeingEdited = false;
                if (_needsScriptInstance)
                {
                    AddScriptInstance(openNodeTree);
                    _needsScriptInstance = false;
                }
                mwvm.ShowAllScripts();
            }
        }

        public void DeleteScript(IScriptInstance script)
        {
            App.LaminarInstance.AllScripts.Scripts.Remove(script);
        }

        public void AddScriptInstance(IAdvancedScript script)
        {
            (DataContext as MainWindowViewModel).CloseAddScriptsButton();
            App.LaminarInstance.AllScripts.Scripts.Add(script.CreateInstance());
        }

        public async Task AddScript()
        {
            string text = await TextPrompt.Show(this, "Script Maker", "Please enter the name of the script");

            if (text is null or "")
            {
                return;
            }

            IAdvancedScript newTree = App.LaminarInstance.Factory.GetImplementation<IAdvancedScript>();
            newTree.Name.Value = text;
            App.LaminarInstance.AllAdvancedScripts.Add(newTree);

            _needsScriptInstance = true;
            OpenScriptEditor(newTree);
        }

        protected override void OnClosed(EventArgs e)
        {
            App.LaminarInstance.Dispose();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UseNativeTitleBar()
        {
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaToDecorationsHint = false;
        }
    }
}
