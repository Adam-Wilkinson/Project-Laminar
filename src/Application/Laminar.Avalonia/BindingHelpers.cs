using Avalonia.Data;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia;

public static class BindingHelpers
{
    extension<T>(IObservableValue<T> observable)
    {
        public CompiledBinding ToBinding() 
            => CompiledBinding.Create((IObservableValue<T> o) => o.Value, source: observable, mode: BindingMode.TwoWay);   
    }

    extension<T>(IReadOnlyObservableValue<T> observable)
    {
        public CompiledBinding ToBinding()
            => CompiledBinding.Create((IReadOnlyObservableValue<T> o) => o.Value, source: observable, mode: BindingMode.OneWay);
    }

    extension<T>(T value)
    {
        public CompiledBinding AsStaticBinding() => new() { Source = value };
    }
}