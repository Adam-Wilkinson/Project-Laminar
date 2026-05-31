using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IWrappedNode : INotificationClient<LaminarExecutionContext>
{
    GuidIdentifier<IWrappedNode> Id { get; }

    INodeRow NameRow { get; }
    
    IReadOnlyObservableCollection<INodeRow> Rows { get; }
    
    ObservableValue<bool> IsCollapsed { get; set; }

    ObservableValue<Point> Location { get; }

    void Update(LaminarExecutionContext context);
}
