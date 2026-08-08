namespace Laminar.Domain.Observables;

public interface INotificationClient<in T>
{
    public void TriggerNotification(T context);
}
