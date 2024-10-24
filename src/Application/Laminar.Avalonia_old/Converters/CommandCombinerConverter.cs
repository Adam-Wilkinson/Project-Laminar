using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using Avalonia.Data.Converters;
using ReactiveUI;

namespace Laminar.Avalonia.Converters;

public class CommandCombinerConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        return ReactiveCommand.CreateCombined(values.Cast<ReactiveCommand<object, Unit>>());
    }
}
