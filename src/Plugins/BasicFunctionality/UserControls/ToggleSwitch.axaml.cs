using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Laminar_Inbuilt.UserControls
{
    public class ToggleSwitch : UserControl
    {
        public ToggleSwitch()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
