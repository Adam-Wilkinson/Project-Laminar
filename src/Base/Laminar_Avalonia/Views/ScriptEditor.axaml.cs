using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Scripting.Advanced.Editing;

namespace Laminar_Avalonia.Views
{
    public class ScriptEditor : UserControl
    {
        public ScriptEditor()
        {
            InitializeComponent();
            DataContext = App.LaminarInstance.Factory.GetImplementation<IAdvancedScript>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
