using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Laminar_Avalonia.Views;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using System.ComponentModel;
using System.Diagnostics;

namespace Laminar_Avalonia.Models
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            ShowAllScripts();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IControl MainControl { get; set; }

        public void ShowScriptEditor(INodeTree script)
        {
            MainControl = new ScriptEditor
            {
                DataContext = script,
            };
            script.EditorIsLive = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainControl)));
        }
        
        public void ShowAllScripts()
        {
            MainControl = new AllScriptsViewer
            {
                DataContext = App.LaminarInstance
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainControl)));
        }

        public void CloseAddScriptsButton()
        {
            if (MainControl is AllScriptsViewer)
            {
                MainControl.FindControl<ToggleButton>("PART_AddScriptButton").IsChecked = false;
            }
        }
    }
}
