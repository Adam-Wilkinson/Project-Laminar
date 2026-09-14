using Laminar.Avalonia.Controls;
using Laminar.Avalonia.Translations;
using Laminar.Domain.Notifications;
using Laminar.Domain.Notifications.Resolutions;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.Localizers;

public static class NotificationLocalizer
{
    public static NotificationDisplayItem Localize(NotificationBase notification) => notification.Type switch
    {
        NotificationType.NodeSourcePluginMissing when notification is ResolvableNotification<VersionedPluginId, NodePluginNotInstalledResolution> n 
            => n.Localize(string.Format(Strings.MissingPlugin.CurrentValue, n.Data.Localize()), 
            0, 
            (Strings.InstallPlugin.CurrentValue, NodePluginNotInstalledResolution.InstallPlugin), 
            (Strings.DeleteNode.CurrentValue, NodePluginNotInstalledResolution.DeleteNode)),
        NotificationType.PluginDoesNotContainNode when notification is DismissableNotification<VersionedPluginId> n 
            => n.Localize(string.Format(Strings.PluginDoesNotContainNode.CurrentValue, n.Data.Localize()), Strings.DeleteNode.CurrentValue),
        NotificationType.RuntimeLoadingPlugin when notification is LifetimeControlledNotification<VersionedPluginId> n 
            => n.Localize(string.Format(Strings.RuntimeLoadingPlugin.CurrentValue, n.Data.Localize())),
        NotificationType.RuntimePluginNotFound when notification is DismissableNotification<VersionedPluginId> n 
            => n.Localize(string.Format(Strings.RuntimePluginNotFound.CurrentValue, n.Data.Localize())),
        NotificationType.LoadingFolderContents when notification is LifetimeControlledNotification n
            => n.Localize(Strings.LoadingFolderContents.CurrentValue),
        _ => throw new ArgumentOutOfRangeException()
    };

    private static NotificationDisplayItem Localize<T>(this ResolvableNotification<T> resolvable, string message, int autoResolveIndex, params ReadOnlySpan<(string, T)> resolutions)
    {
        List<NotificationResolution> notificationResolutions = new(resolutions.Length);
        foreach (var (text, parameter) in resolutions)
        {
            notificationResolutions.Add(new NotificationResolution(text, () => resolvable.Resolve(parameter)));
        }

        return new NotificationDisplayItem(resolvable.Severity, message, notificationResolutions, null, autoResolveIndex);
    }

    private static NotificationDisplayItem Localize(this DismissableNotification dismissable, string message, string? dismissMessage = null) 
        => dismissMessage is null 
            ? new(dismissable.Severity, message, [], dismissable.Dismiss)
            : new(dismissable.Severity, message, [new NotificationResolution(dismissMessage, dismissable.Dismiss)], null, 0);

    private static NotificationDisplayItem Localize(this LifetimeControlledNotification lifetimeControlled, string message)
        => new(lifetimeControlled.Severity, message, []);

    private static string Localize(this VersionedPluginId plugin) 
        => string.Format(Strings.PluginVersion.CurrentValue, plugin.Name, plugin.Version);
}