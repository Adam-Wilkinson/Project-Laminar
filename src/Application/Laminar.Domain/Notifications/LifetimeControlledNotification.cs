namespace Laminar.Domain.Notifications;

public abstract class LifetimeControlledNotification<TData> : LifetimeControlledNotification
{
    public abstract TData Data { get; }
}

public abstract class LifetimeControlledNotification : NotificationBase, IDisposable
{
    public void Dispose()
    {
        DismissInternal();
        GC.SuppressFinalize(this);
    }
}