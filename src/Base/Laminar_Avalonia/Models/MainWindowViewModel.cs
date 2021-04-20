using Avalonia.Controls;
using Laminar_Avalonia.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Avalonia.Models
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IControl MainControl { get; private set; }

        public void ShowScriptEditor()
        {
            Debug.WriteLine(App.LaminarInstance.ActiveNodeTree.Value.Name.Value);
            MainControl = new ScriptEditor
            {
                DataContext = App.LaminarInstance.ActiveNodeTree.Value
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainControl)));
        }

        public void ShowAllScripts()
        {
            MainControl = new AllScriptsViewer
            {
                DataContext = App.LaminarInstance.ActiveNodeTree
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainControl)));
        }
    }
}
