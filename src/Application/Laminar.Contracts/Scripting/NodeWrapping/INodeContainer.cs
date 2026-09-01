using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notifications;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeContainer : IDisposable
{
    public class PluginMissingNotificationTemplate(VersionedPluginId plugin)
        : NotificationTemplate(NotificationSeverity.Error, $"This node is from the plugin '{plugin.Name}' (Version {plugin.Version}), which is not installed");

    public class PluginDoesNotContainNodeNotificationTemplate(VersionedPluginId plugin, string nodeName)
        : NotificationTemplate(NotificationSeverity.Error, $"Unable to find node '{nodeName}' in plugin {plugin.Name}' (Version {plugin.Version})");
    
    public INodeRow<IInterfaceData<EditableLabel, string>> NameRow { get; }

    public IReadOnlyObservableCollection<INodeRow> Rows { get; }
    
    public IObservableValue<bool> IsCollapsed { get; }
    
    public IObservableValue<Point> Location { get; }
    
    public NodeDescriptor Descriptor { get; }
    
    public IEncodableData PersistentData { get; }
    
    public NotificationManager Notifications { get; }

    public IRuntimeNodeInstance? RuntimeNode { get; }
}