using Laminar.Domain.Notifications;
using Laminar.Domain.Notifications.Resolutions;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Notifications;

public class PluginNotInstalledNotification(VersionedPluginId plugin) 
    : ResolvableNotification<VersionedPluginId, NodePluginNotInstalledResolution>
{
    public override NotificationSeverity Severity => NotificationSeverity.Error;
    public override NotificationType Type => NotificationType.NodeSourcePluginMissing;

    public override VersionedPluginId Data { get; } = plugin;
    
    protected override void ResolveOverride(NodePluginNotInstalledResolution parameter)
    {
        // Implementation. Knows all about the things it needs
    }
}