namespace Laminar.Domain.Notifications;

public abstract class DismissableNotification<TData> : DismissableNotification
{
    public abstract TData Data { get; }
}

public abstract class DismissableNotification : NotificationBase
{
    public void Dismiss()
    {
        DismissInternal();
    }   
}