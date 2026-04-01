using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Laminar.PluginFramework.UserInterface;
using AvaloniaColor = Avalonia.Media.Color;
using DrawingColor = System.Drawing.Color;

namespace BasicFunctionality.Avalonia.UserControls;

public partial class ColorViewer : UserControl
{
    private static readonly DrawingColorToBrushConverter ColorToBrushConverter = new();
    
    public ColorViewer()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IInterfaceData { Definition: Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions.ColorViewer viewerDefinition } interfaceData)
        {
            ColorDisplayRect[!Shape.FillProperty] = new Binding { Path = nameof(IInterfaceData.Value), Converter = ColorToBrushConverter };
        }
    }

    private class DrawingColorToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is AvaloniaColor avaloniaColor)
            {
                return new SolidColorBrush(avaloniaColor);
            }
            
            if (value is DrawingColor drawingColor)
            {
                return new SolidColorBrush(new AvaloniaColor(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B));
            }
            
            return new BindingNotification(
                    new ArgumentException($"Value must be of a known color type, not {value?.GetType()}", nameof(value)),
                    BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not SolidColorBrush { Color: var color} )
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
            
            return new BindingNotification(new ArgumentException($"Unknown color type: {targetType}", nameof(targetType)),  BindingErrorType.Error);
        }
    }
}