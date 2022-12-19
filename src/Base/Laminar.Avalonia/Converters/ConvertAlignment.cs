namespace Laminar_Avalonia.Converters
{
    using System;
    using System.Globalization;
    using Avalonia.Data.Converters;
    using Avalonia.Layout;

    public class ConvertAlignment : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value) switch
            {
                Laminar_Core.Primitives.HorizontalAlignment.Left => HorizontalAlignment.Left,
                Laminar_Core.Primitives.HorizontalAlignment.Right => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Stretch,
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
