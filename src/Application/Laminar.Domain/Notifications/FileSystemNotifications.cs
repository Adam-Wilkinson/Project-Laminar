namespace Laminar.Domain.Notifications;

public static class FileSystemNotifications
{
    public static readonly Notification LoadingFolderContents = new(NotificationSeverity.Loading, "Loading folder contents");
    
    public static readonly Notification LoadingRequiredPlugins = new(NotificationSeverity.Loading, "Loading required plugins");

    public record PluginOnlyLocalWarning(string PluginId) : Notification(NotificationSeverity.Warning, $"The plugin {PluginId} cannot be found on any repositories, but exists offline");

    public record PluginNotFoundError(string PluginId) : Notification(NotificationSeverity.Error, $"The plugin {PluginId} cannot be loaded");
}