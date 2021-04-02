using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenFlow_Inbuilt.UserControls
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
