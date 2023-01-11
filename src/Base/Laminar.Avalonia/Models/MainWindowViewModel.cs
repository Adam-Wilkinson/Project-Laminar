using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Laminar.Avalonia.Views;
using Laminar.Contracts.Scripting;
using System.ComponentModel;

namespace Laminar.Avalonia.Models
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            ShowAllScripts();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IControl MainControl { get; set; }

        public void ShowScriptEditor(IScript script)
        {
            MainControl = new ScriptEditor();
            MainControl.DataContext = script;

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
