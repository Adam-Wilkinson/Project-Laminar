using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Laminar_PluginFramework.Primitives;
using System.Diagnostics;

namespace Laminar_Inbuilt.UserControls
{
    public class EnumEditor : UserControl
    {
        private ComboBox _combobox;

        public EnumEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            DataContextChanged += EnumEditor_DataContextChanged;
            _combobox = this.FindControl<ComboBox>("CBox");
            _combobox.PointerPressed += (o, e) => { e.Handled = true; };
        }

        private void EnumEditor_DataContextChanged(object sender, System.EventArgs e)
        {
            _combobox.Items = (DataContext as ILaminarValue)?.TypeDefinition.ValueType.GetEnumValues();
        }
    }
}
