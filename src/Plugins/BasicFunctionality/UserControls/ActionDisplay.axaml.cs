using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BasicFunctionality.UserControls
{
    public class ActionDisplay : UserControl
    {
        public ActionDisplay()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
