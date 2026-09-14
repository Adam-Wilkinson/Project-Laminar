using Laminar.Domain.Notifications;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Notifications;

public class PluginNotFoundNotification(VersionedPluginId pluginId) : DismissableNotification<VersionedPluginId>
{
    public override NotificationSeverity Severity => NotificationSeverity.Warning;
    
    public override NotificationType Type => NotificationType.RuntimePluginNotFound;
    
    public override VersionedPluginId Data { get; } = pluginId;
}