using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Notifications;

public static class NodeNotifications
{
    public class PluginNotInstalled(VersionedPluginId plugin)
        : NotificationTemplate<PluginNotInstalledResolution>(NotificationSeverity.Error,
            $"This node is from the plugin '{plugin.Name}' (Version {plugin.Version}), which is not installed")
    {
        public VersionedPluginId Plugin { get; } = plugin;
    }
    
    public enum PluginNotInstalledResolution
    {
        InstallPlugin,
        DeleteNode
    }
    
    public class PluginDoesNotContainNode(VersionedPluginId plugin, string nodeName)
        : NotificationTemplate<DeleteNodeResolution>(NotificationSeverity.Error, $"Unable to find node '{nodeName}' in plugin {plugin.Name}' (Version {plugin.Version})");
    
    public enum DeleteNodeResolution
    {
        DeleteNode,
    }
}