using System.ComponentModel;
using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notification;

/// <summary>
/// Maps a regular function into one that returns an <see cref="IReadOnlyObservableValue{T}"/> which automatically updates when the appropriate properties are updates in the input
/// </summary>
/// <typeparam name="TInput">The input type of the function. The output will be reactive if this implements <see cref="INotifyPropertyChanged"/></typeparam>
/// <typeparam name="TOutput">The output type of the function</typeparam>
public class ReactiveFunc<TInput, TOutput>
{
    private readonly HashSet<string>? _ignoredProperties;
    private readonly HashSet<string>? _dependentProperties;
    private readonly Func<TInput, TOutput> _func;
    
    public ReactiveFunc(Func<TInput, TOutput> func, params Span<string> dependentProperties)
    {
        if (typeof(TInput).GetInterface(nameof(INotifyPropertyChanged)) is not null)
        {
            if (dependentProperties.Length > 0)
            {
                _dependentProperties = [..dependentProperties];
            }
            else
            { 
                _ignoredProperties = [];
            }
        }

        _func = func;
    }
    
    public Func<TInput, IReadOnlyObservableValue<TOutput>> AsFunc() => GetObservable; 
    
    public IReadOnlyObservableValue<TOutput> GetObservable(TInput input)
    {
        var result = new ObservableValue<TOutput>(_func(input));
        if (input is INotifyPropertyChanged notifier)
        {
            notifier.PropertyChanged += (_, args) => OnNotifierPropertyChanged(result, input, args.PropertyName); 
        }

        return result;
    }

    private void OnNotifierPropertyChanged(ObservableValue<TOutput> value, TInput input, string? propertyName)
    {
        if (propertyName is null || _ignoredProperties!.Contains(propertyName)) 
            return;

        if (_dependentProperties is not null && _dependentProperties.Contains(propertyName))
        {
            value.Value = _func(input);
            return;
        }
        
        var oldValue = value.Value;
        var newValue = _func(input);
        if (EqualityComparer<TOutput>.Default.Equals(oldValue, newValue))
        {
            _ignoredProperties.Add(propertyName);
            return;
        }

        value.Value = newValue;
    }
}