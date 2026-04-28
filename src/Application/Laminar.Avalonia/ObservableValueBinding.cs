using Avalonia.Data;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia;

public static class ObservableValueBinding
{
    extension<T>(IObservableValue<T> observable)
    {
        public CompiledBinding ToBinding() 
            => CompiledBinding.Create<IObservableValue<T>, T>(x => x.Value, source: observable, mode: BindingMode.TwoWay);   
    }
}