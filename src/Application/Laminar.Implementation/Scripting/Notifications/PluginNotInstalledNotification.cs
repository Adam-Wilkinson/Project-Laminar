using Laminar.Domain.Notifications;
using Laminar.Domain.Notifications.Resolutions;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Actions;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Notifications;

internal class PluginNotInstalledNotification(
    NodeContainer nodeContainer,
    VersionedPluginId plugin) 
    : ResolvableNotification<VersionedPluginId, NodePluginNotInstalledResolution>
{
    public override NotificationSeverity Severity => NotificationSeverity.Error;
    
    public override NotificationType Type => NotificationType.NodeSourcePluginMissing;

    public override VersionedPluginId Data { get; } = plugin;
    
    protected override void ResolveOverride(NodePluginNotInstalledResolution parameter)
    {
        switch (parameter)
        {
            case NodePluginNotInstalledResolution.DeleteNode:
                if (nodeContainer.Host is null) throw new InvalidOperationException("Cannot delete node without a host");
                nodeContainer.Host.ActionScope.ExecuteAction(new DeleteNodeAction(nodeContainer, nodeContainer.Host.Nodes));
                break;
            case NodePluginNotInstalledResolution.InstallPlugin:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
        }
    }
}