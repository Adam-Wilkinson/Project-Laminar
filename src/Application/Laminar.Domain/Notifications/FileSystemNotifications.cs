using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notifications;

public static class FileSystemNotifications
{
    public class LoadingFolderContents() : NotificationTemplate(NotificationSeverity.Loading, "Loading folder contents");
    
    public class LoadingRequiredPlugin(string pluginId, SemanticVersion pluginVersion) 
        : NotificationTemplate(NotificationSeverity.Loading, $"Loading plugin: {pluginId} (Version {pluginVersion})");

    public class PluginOnlyLocalWarning(string pluginId, SemanticVersion version) : NotificationTemplate(NotificationSeverity.Warning,
        $"The plugin {pluginId} (Version {version}) cannot be found on any repositories, but exists offline")
    {
        public string PluginId { get; init; } = pluginId;
    }

    public class PluginNotFoundError(string pluginId, SemanticVersion version) : NotificationTemplate(NotificationSeverity.Error, 
        $"The plugin {pluginId} (Version {version}) could not be loaded")
    {
        public string PluginId { get; init; } = pluginId;
    }
}