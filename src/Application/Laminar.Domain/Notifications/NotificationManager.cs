using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Domain.Notifications;

public sealed class NotificationManager : INotifyPropertyChanged
{
    private readonly ObservableCollection<NotificationBase> _allNotifications = [];
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public NotificationSeverity MaximumSeverity 
    { 
        get;
        private set => SetField(ref field, value);
    } = NotificationSeverity.None;
    
    public IReadOnlyObservableList<NotificationBase> AllNotifications => field ??= _allNotifications.ToObservableList();

    public IDisposable AddNotification(LifetimeControlledNotification notification)
    {
        AddNotification((NotificationBase)notification);
        return notification;
    }
    
    public void AddNotification(NotificationBase notification)
    {
        _allNotifications.Add(notification);
        OnSeverityAdded(notification.Severity);
        notification.Dismissed += NotificationOnDismissed;
    }

    private void NotificationOnDismissed(object? sender, EventArgs e)
    {
        if (sender is not NotificationBase notification)
        {
            throw new InvalidOperationException("Dismissal must come from a NotificationBase");
        }
        
        RemoveNotification(notification);
    }

    private void RemoveNotification(NotificationBase notification)
    {
        _allNotifications.Remove(notification);
        OnSeverityRemoved(notification.Severity);
        notification.Dismissed -= NotificationOnDismissed;
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
                : (NotificationSeverity)AllNotifications.Max(x => (int)x.Severity);
        }
    }
    
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}