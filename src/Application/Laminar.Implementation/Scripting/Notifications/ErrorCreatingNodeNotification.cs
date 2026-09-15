using Laminar.Domain.Notifications;

namespace Laminar.Implementation.Scripting.Notifications;

public class ErrorCreatingNodeNotification(string nodeName) : LifetimeControlledNotification<string>
{
    public override NotificationSeverity Severity => NotificationSeverity.Error;
    public override NotificationType Type => NotificationType.ErrorCreatingNode;
    public override string Data { get; } = nodeName;
}