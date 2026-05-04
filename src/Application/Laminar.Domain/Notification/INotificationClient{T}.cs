namespace Laminar.Domain.Notification;

public interface INotificationClient<in T>
{
    public void TriggerNotification(T value);
}
