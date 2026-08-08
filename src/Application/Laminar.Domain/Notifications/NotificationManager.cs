using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Domain.Notifications;

public sealed class NotificationManager : IDisposable, INotifyPropertyChanged
{
    private readonly IDisposable _notificationCollectionSubscription;
    private bool _isDisposed;
    
    public NotificationManager()
    {
        _notificationCollectionSubscription = AllNotifications.SubscribeForEach(
            OnNotificationAdded, OnNotificationRemoved);
    }

    public NotificationSeverity MaximumSeverity 
    { 
        get;
        private set => SetField(ref field, value);
    } = NotificationSeverity.None;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnNotificationRemoved(INotification notification)
    {
        MaximumSeverity = (NotificationSeverity)Math.Max((int)notification.Severity, (int)MaximumSeverity);
    }

    private void OnNotificationAdded(INotification notification)
    {
        if (notification.Severity == MaximumSeverity)
        {
            MaximumSeverity = (NotificationSeverity)AllNotifications.Max(x => (int)x.Severity);
        }
    }

    public IObservableCollection<INotification> AllNotifications { get; } = new ObservableCollectionImpl<INotification>([]);

    public IDisposable ScopeNotification(INotification notification) => new NotificationScope(notification, this);

    private sealed class NotificationScope : IDisposable
    {
        private readonly INotification _notification;
        private readonly NotificationManager _collection;

        public NotificationScope(INotification notification, NotificationManager collection)
        {
            _notification = notification;
            _collection = collection;
            _collection.AllNotifications.Remove(_notification);
        }

        public void Dispose()
        {
            _collection.AllNotifications.Remove(_notification);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _notificationCollectionSubscription.Dispose();
        AllNotifications.Clear();
    }


    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public record Notification(NotificationSeverity Severity, string Message) : INotification;

public interface INotification
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