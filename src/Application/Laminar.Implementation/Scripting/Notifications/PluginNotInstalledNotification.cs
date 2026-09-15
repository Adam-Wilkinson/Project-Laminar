using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Notifications;
using Laminar.Domain.Notifications.Resolutions;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Actions;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Notifications;

internal class PluginNotInstalledNotification : ResolvableNotification<VersionedPluginId, NodePluginNotInstalledResolution>
{
    private readonly NodeContainer _nodeContainer;
    private readonly IDisposable _pluginAddedSubscription;

    public PluginNotInstalledNotification(NodeContainer nodeContainer, VersionedPluginId plugin, IPluginManager pluginManager)
    {
        _nodeContainer = nodeContainer;
        Data = plugin;
        _pluginAddedSubscription = pluginManager.Plugins.SubscribeForEach(OnPluginAdded);
    }

    public override NotificationSeverity Severity => NotificationSeverity.Error;
    
    public override NotificationType Type => NotificationType.NodeSourcePluginMissing;

    public override VersionedPluginId Data { get; }

    protected override void ResolveOverride(NodePluginNotInstalledResolution parameter)
    {
        if (_nodeContainer.Host is not { } nodeHost) throw new InvalidOperationException("Cannot delete node without a host");
        switch (parameter)
        {
            case NodePluginNotInstalledResolution.DeleteNode:
                nodeHost.ActionScope.ExecuteAction(new DeleteNodeAction(_nodeContainer, nodeHost.Nodes));
                break;
            case NodePluginNotInstalledResolution.InstallPlugin:
                nodeHost.Runtime.PluginManager.EnsurePluginInstalled(Data);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
        }
    }

    protected override void OnDismissed()
    {
        _pluginAddedSubscription.Dispose();
    }

    private void OnPluginAdded(IInstalledPlugin plugin)
    {
        if (plugin.PluginId == Data)
        {
            DismissInternal();
        }
    }
}