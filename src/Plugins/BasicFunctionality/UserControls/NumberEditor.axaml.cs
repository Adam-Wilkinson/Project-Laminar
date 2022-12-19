using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BasicFunctionality.UserControls
{
    public class NumberEditor : UserControl
    {
        public NumberEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
