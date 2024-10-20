namespace Laminar.Contracts.Notification;

public interface INotificationClient<in T>
{
    public void TriggerNotification(T value);
}
