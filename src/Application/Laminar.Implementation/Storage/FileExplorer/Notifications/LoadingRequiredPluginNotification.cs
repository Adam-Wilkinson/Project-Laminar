using Laminar.Domain.Notifications;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Notifications;

public class LoadingRequiredPluginNotification(VersionedPluginId plugin) : LifetimeControlledNotification<VersionedPluginId>
{
    public override NotificationSeverity Severity => NotificationSeverity.Loading;
    
    public override NotificationType Type => NotificationType.RuntimeLoadingPlugin;

    public override VersionedPluginId Data { get; } = plugin;
}