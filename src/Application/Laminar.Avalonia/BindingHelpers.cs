using Avalonia.Data;
using Laminar.Domain.Notification.Value;

namespace Laminar.Avalonia;

public static class BindingHelpers
{
    extension<T>(IObservableValue<T> observable)
    {
        public CompiledBinding ToBinding(BindingMode mode = BindingMode.TwoWay, BindingPriority priority = BindingPriority.LocalValue) 
            => CompiledBinding.Create((IObservableValue<T> o) => o.Value, source: observable, mode: mode, priority: priority);   
    }

    extension<T>(T value)
    {
        public CompiledBinding AsStaticBinding(BindingPriority priority = BindingPriority.LocalValue) => new()
        {
            Source = value,
            Priority = priority,
            Mode = BindingMode.OneWay
        };
    }
}