using Laminar.Domain.Notifications;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Notifications;

public class PluginDoesNotContainNodeNotification(VersionedPluginId plugin) : DismissableNotification<VersionedPluginId>
{
    public override NotificationSeverity Severity => NotificationSeverity.Error;
    
    public override NotificationType Type => NotificationType.PluginDoesNotContainNode;
    
    public override VersionedPluginId Data { get; } = plugin;
}