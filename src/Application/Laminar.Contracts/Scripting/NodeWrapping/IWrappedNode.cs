using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IWrappedNode : INotificationClient<LaminarExecutionContext>
{
    GuidIdentifier<IWrappedNode> Id { get; }

    INodeRow<IInterfaceData<EditableLabel, string>> NameRow { get; }
    
    IReadOnlyObservableCollection<INodeRow> Rows { get; }
    
    IObservableValue<bool> IsCollapsed { get; }

    IObservableValue<Point> Location { get; }

    public byte[] ToPersistentValue(IPersistentDataTranscoder dataTranscoder);
    
    void Update(LaminarExecutionContext context);
}
