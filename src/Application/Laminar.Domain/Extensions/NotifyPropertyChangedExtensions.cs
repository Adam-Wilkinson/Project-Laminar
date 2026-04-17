using System.ComponentModel;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Extensions;

public static class NotifyPropertyChangedExtensions
{
    private static readonly Dictionary<INotifyPropertyChanged, PropertyChangedHolder> Holders = [];

    public static IReadOnlyObservableValue<TValue> GetDependentValue<TObject, TValue>(
        this TObject notifyPropertyChanged, Func<TObject, TValue> dependent, IEqualityComparer<TValue>? equalityComparer = null)
        where TObject : INotifyPropertyChanged
    {
        PropertyChangedDependency<TObject, TValue> newDependentValue = new(dependent(notifyPropertyChanged), dependent, equalityComparer);
        GetHolder(notifyPropertyChanged).AddDependency(newDependentValue);
        return newDependentValue;
    }
    
    public static INotificationSource FilterPropertyChanged(this INotifyPropertyChanged notifyPropertyChanged, string propertyName)
    {
        return GetHolder(notifyPropertyChanged).GetFilter(propertyName);
    }

    private static PropertyChangedHolder GetHolder(INotifyPropertyChanged notifyPropertyChanged)
    {
        if (Holders.TryGetValue(notifyPropertyChanged, out var holder))
        {
            return holder;
        }

        var newHolder = new PropertyChangedHolder(notifyPropertyChanged);
        Holders.Add(notifyPropertyChanged, newHolder);
        return newHolder;
    }

    private class PropertyChangedHolder
    {
        private readonly Dictionary<string, FilteredPropertyChangedEventSource> _filteredEvents = [];
        private readonly List<IPropertyChangedDependency> _dependencies = [];
        
        public PropertyChangedHolder(INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName is not null && _filteredEvents.TryGetValue(args.PropertyName, out var filteredEvent))
                {
                    filteredEvent.InvokePropertyChanged(sender, args);
                }

                foreach (var dependency in _dependencies)
                {
                    dependency.InvokeIfChanged(sender, notifyPropertyChanged);
                }
            };
        }

        public void AddDependency(IPropertyChangedDependency dependency)
        {
            _dependencies.Add(dependency);
        }
        
        public FilteredPropertyChangedEventSource GetFilter(string propertyName)
        {
            if (_filteredEvents.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            var newFilter = new FilteredPropertyChangedEventSource();
            _filteredEvents.Add(propertyName, newFilter);
            return newFilter;
        }
    }
    
    private class FilteredPropertyChangedEventSource : INotifyPropertyChanged, INotificationSource
    {
        public void InvokePropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(sender, args);
            OnNotification?.Invoke(sender, EventArgs.Empty);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? OnNotification;
    }
}

public class PropertyChangedDependency<TObject, TValue>(TValue initialValue, Func<TObject, TValue> dependentValue, IEqualityComparer<TValue>? comparer) 
    : ObservableValue<TValue>(initialValue), IPropertyChangedDependency
{
    private readonly IEqualityComparer<TValue> _comparer = comparer ?? EqualityComparer<TValue>.Default;
    private readonly Lock _computeChangeLock = new();
    
    void IPropertyChangedDependency.InvokeIfChanged(object? sender, INotifyPropertyChanged notifyPropertyChanged)
    {
        if (notifyPropertyChanged is not TObject typedObject) return;

        TValue newValue;
        
        lock (_computeChangeLock)
        {
            TValue oldValue = Value;
            newValue = dependentValue(typedObject);

            if (_comparer.Equals(oldValue, newValue)) return;
        }
        
        Value = newValue;
    }
}

internal interface IPropertyChangedDependency
{
    internal void InvokeIfChanged(object? sender, INotifyPropertyChanged notifyPropertyChanged);
}