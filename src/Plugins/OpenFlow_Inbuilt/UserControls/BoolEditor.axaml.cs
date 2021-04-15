using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenFlow_Inbuilt.UserControls
{
    public class BoolEditor : UserControl
    {
        public BoolEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
