using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Notifications;

internal class NodeNotificationFactory
{
    public PluginNotInstalledNotification PluginNotInstalled(VersionedPluginId plugin, NodeContainer node, IPluginManager pluginManager) 
        => new(node, plugin, pluginManager);
    
    public PluginDoesNotContainNodeNotification PluginDoesNotContainNode(VersionedPluginId plugin, NodeContainer node)
        => new(node, plugin);
}