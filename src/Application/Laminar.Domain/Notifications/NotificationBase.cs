namespace Laminar.Domain.Notifications;

public abstract class NotificationBase
{
    public abstract NotificationSeverity Severity { get; }
    public abstract NotificationType Type { get; }
    
    public event EventHandler? Dismissed;

    protected virtual void OnDismissed()
    {
    }
    
    protected void DismissInternal()
    {
        OnDismissed();
        Dismissed?.Invoke(this, EventArgs.Empty);
    }
}