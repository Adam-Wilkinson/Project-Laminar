namespace Laminar.Contracts.Primitives;

public interface INotificationClient<in T>
{
    public void TriggerNotification(T value);
}
