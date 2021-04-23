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

        public IControl MainControl { get; private set; }

        public void ShowScriptEditor(INodeTree script)
        {
            MainControl = new ScriptEditor
            {
                DataContext = script,
            };
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
            MainControl.FindControl<ToggleButton>("PART_AddScriptButton").IsChecked = false;
        }
    }
}
