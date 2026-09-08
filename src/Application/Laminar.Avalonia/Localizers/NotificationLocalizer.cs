using Laminar.Avalonia.Controls;
using Laminar.Domain.Notifications;
using static Laminar.Domain.Notifications.NodeNotifications;

namespace Laminar.Avalonia.Localizers;

public class NotificationLocalizer
{
    public static NotificationDisplayItem Localize(INotification notification) => notification switch
    {
        INotification<PluginNotInstalledResolution> {Template: PluginNotInstalled {Plugin: var plugin}} pni 
            => new NotificationDisplayItem(pni.Template.Severity, $"Missing plugin: {plugin.Name} (v{plugin.Version})", [
                new NotificationResolution("Install", () => pni.ResolveWithParameter(PluginNotInstalledResolution.InstallPlugin)),
                new NotificationResolution("Delete", () => pni.ResolveWithParameter(PluginNotInstalledResolution.DeleteNode))
            ], 0), 
        
        var unknown => new NotificationDisplayItem(notification.Template.Severity, notification.Template.Message, [
            new NotificationResolution("Dismiss", notification.Dispose)
        ], 0)
    };
}