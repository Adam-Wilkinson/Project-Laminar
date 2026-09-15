using Laminar.Domain.Notifications;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Actions;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Notifications;

internal class PluginDoesNotContainNodeNotification(
    NodeContainer nodeContainer,
    VersionedPluginId plugin) 
    : DismissableNotification<VersionedPluginId>
{
    public override NotificationSeverity Severity => NotificationSeverity.Error;
    
    public override NotificationType Type => NotificationType.PluginDoesNotContainNode;
    
    public override VersionedPluginId Data { get; } = plugin;

    protected override void OnDismissed()
    {
        if (nodeContainer.Host is not { } nodeHost) return;

        nodeHost.ActionScope.ExecuteAction(new DeleteNodeAction(nodeContainer, nodeHost.Nodes));
    }
}