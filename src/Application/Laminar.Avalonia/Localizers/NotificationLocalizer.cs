using Laminar.Avalonia.Controls;
using Laminar.Avalonia.Translations;
using Laminar.Domain.Notifications;
using static Laminar.Domain.Notifications.NodeNotifications;

namespace Laminar.Avalonia.Localizers;

public class NotificationLocalizer
{
    public static NotificationDisplayItem Localize(INotification notification) => notification switch
    {
        INotification<PluginNotInstalledResolution> {Template: PluginNotInstalled {Plugin: var plugin}} pni 
            => new NotificationDisplayItem(pni.Template.Severity, string.Format(Strings.MissingPlugin.CurrentValue, plugin.Name, plugin.Version), [
                new NotificationResolution(Strings.InstallPlugin.CurrentValue, () => pni.ResolveWithParameter(PluginNotInstalledResolution.InstallPlugin)),
                new NotificationResolution(Strings.DeleteNode.CurrentValue, () => pni.ResolveWithParameter(PluginNotInstalledResolution.DeleteNode))
            ], 0), 
        
        var unknown => new NotificationDisplayItem(notification.Template.Severity, notification.Template.Message, [
            new NotificationResolution(Strings.DismissNotification.CurrentValue, notification.Dispose)
        ], 0)
    };
}