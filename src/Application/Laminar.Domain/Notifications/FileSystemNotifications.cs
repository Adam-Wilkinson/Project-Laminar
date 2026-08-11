using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notifications;

public static class FileSystemNotifications
{
    public class LoadingFolderContents() : NotificationTemplate(NotificationSeverity.Loading, "Loading folder contents");
    
    public class LoadingRequiredPlugin(VersionedPluginId pluginId) 
        : NotificationTemplate(NotificationSeverity.Loading, $"Loading plugin: {pluginId.Name} (Version {pluginId.Version})");

    public class PluginOnlyLocalWarning(VersionedPluginId pluginId) : NotificationTemplate(NotificationSeverity.Warning,
        $"The plugin {pluginId.Name} (Version {pluginId.Version}) cannot be found on any repositories, but exists offline")
    {
        public VersionedPluginId PluginId { get; init; } = pluginId;
    }

    public class PluginNotFoundError(VersionedPluginId pluginId) : NotificationTemplate(NotificationSeverity.Error, 
        $"The plugin {pluginId.Name} (Version {pluginId.Version}) could not be loaded")
    {
        public VersionedPluginId PluginId { get; init; } = pluginId;
    }
}