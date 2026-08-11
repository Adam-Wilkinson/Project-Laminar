using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Domain.Notifications;

public sealed class NotificationManager : INotifyPropertyChanged
{
    private readonly ObservableCollection<INotification> _allNotifications = [];
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public NotificationSeverity MaximumSeverity 
    { 
        get;
        private set => SetField(ref field, value);
    } = NotificationSeverity.None;
    
    public IReadOnlyObservableCollection<INotification> AllNotifications => _allNotifications.ToInterfaceImpl();

    public INotification AddNotification(NotificationTemplate notificationTemplate, Action? onDisposed = null)
    {
        var ret = new Notification(this)
        {
            Template = notificationTemplate,
            OnDisposed = onDisposed
        };
        OnSeverityAdded(notificationTemplate.Severity);
        return ret;
    }

    public INotification<T> AddNotification<T>(INotificationTemplate<T> notificationTemplate, Action<T> resolveAction)
    {
        var ret = new Notification<T>(this)
        {
            Template = notificationTemplate,
            ResolveMethod = resolveAction,
        };
        OnSeverityAdded(notificationTemplate.Severity);
        return ret;
    }
    
    private void OnSeverityAdded(NotificationSeverity severity)
    {
        MaximumSeverity = (NotificationSeverity)Math.Max((int)MaximumSeverity, (int)severity);
    }

    private void OnSeverityRemoved(NotificationSeverity severity)
    {
        if (severity == MaximumSeverity)
        {
            MaximumSeverity = AllNotifications.Count == 0
                ? NotificationSeverity.None
                : (NotificationSeverity)AllNotifications.Max(x => (int)x.Template.Severity);
        }
    }
    
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private sealed class Notification<T>(NotificationManager manager)
        : Notification(manager), INotification<T>
    {
        public required Action<T> ResolveMethod { get; init; }

        public void ResolveWithParameter(T parameter)
        {
            ResolveMethod.Invoke(parameter);
            Dispose();
        }
    } 
    
    private class Notification : INotification
    {
        private readonly NotificationManager _manager;
        
        public required INotificationTemplate Template { get; init; }

        public Action? OnDisposed { get; init; }
        
        public Notification(NotificationManager manager)
        {
            _manager = manager;
            manager._allNotifications.Add(this);
        }

        public void Dispose()
        {
            OnDisposed?.Invoke();
            _manager._allNotifications.Remove(this);
            _manager.OnSeverityRemoved(Template.Severity);
        }
    }
}

public interface INotification<in T> : INotification
{
    public void ResolveWithParameter(T parameter);
}

public interface INotification : IDisposable
{
    public INotificationTemplate Template { get; }
}

public abstract class INotificationTemplate<T>(NotificationSeverity severity, string message) : INotificationTemplate
{
    public NotificationSeverity Severity { get; } = severity;
    public string Message { get; } = message;
}

public abstract class NotificationTemplate(NotificationSeverity severity, string message) : INotificationTemplate
{
    public NotificationSeverity Severity { get; } = severity;
    public string Message { get; } = message;
}

public interface INotificationTemplate
{
    public NotificationSeverity Severity { get; }
    
    public string Message { get; }
}

public enum NotificationSeverity
{
    None = 0,
    Information = 1,
    Loading = 2,
    Warning = 3,
    Error = 4,
}