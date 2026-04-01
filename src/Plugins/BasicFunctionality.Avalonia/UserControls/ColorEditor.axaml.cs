using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Laminar.PluginFramework.UserInterface;
using AvaloniaColor = Avalonia.Media.Color;
using DrawingColor = System.Drawing.Color;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class ColorEditor : UserControl
{
    private static readonly DrawingColorConverter ColorConverter = new();

    public ColorEditor()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IInterfaceData { Definition: Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.ColorEditor viewerDefinition } interfaceData)
        {
            MainColorPicker[!ColorView.ColorProperty] = new Binding { Path = nameof(IInterfaceData.Value), Converter = ColorConverter };
        }
    }

    private class DrawingColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is AvaloniaColor avaloniaColor)
            {
                return avaloniaColor;
            }

            if (value is DrawingColor drawingColor)
            {
                return new AvaloniaColor(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
            }

            return new BindingNotification(
                new ArgumentException($"Value must be of type System.Drawing.Color, not {value?.GetType()}", nameof(value)),
                BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not AvaloniaColor color)
            {
                return new BindingNotification(
                    new ArgumentException($"Value must be of type Avalonia.Media.Color, not {value?.GetType()}", nameof(value)),
                    BindingErrorType.Error);
            }

            if (targetType == typeof(AvaloniaColor))
            {
                return color;
            }

            if (targetType == typeof(DrawingColor))
            {
                return DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
            }
            
            return new BindingNotification(new ArgumentException($"Unknown color target type {targetType}", nameof(targetType)),  BindingErrorType.Error);
        }
    }
}