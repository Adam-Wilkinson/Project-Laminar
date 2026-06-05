using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.Markup;

public static class DependentObservableBinding
{
    public static BindingBase Create<TDependency, TOutput>(BindingBase dependencyBinding, IDependentObservable<TDependency, TOutput> observable)
    {
        return new MultiBinding
        {
            Bindings =
            [
                dependencyBinding,
                observable.ToBinding()
            ],
            Converter = new DependentObservableBindingConverter<TDependency, TOutput>(),
            ConverterParameter = observable,
        };
    }
}

public interface IDependentObservable<TDependency, out TValue> : IObservable<TValue>
{
    public TDependency? Dependency { get; set; }
}

public class DependentObservableBindingConverter<TDependency, TValue> : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (Equals(values[0], AvaloniaProperty.UnsetValue) || Equals(values[1], AvaloniaProperty.UnsetValue))
        {
            return AvaloniaProperty.UnsetValue;
        }
        
        if (parameter is not IDependentObservable<TDependency, TValue> observer || values[0] is not TDependency dependency || values[1] is not TValue returnValue)
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        if (!Equals(observer.Dependency, dependency))
        {
            observer.Dependency = dependency;
        }
        
        return returnValue;
    }
}