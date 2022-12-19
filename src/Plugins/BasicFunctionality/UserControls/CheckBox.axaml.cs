using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BasicFunctionality.UserControls
{
    public class CheckBox : UserControl
    {
        public CheckBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
