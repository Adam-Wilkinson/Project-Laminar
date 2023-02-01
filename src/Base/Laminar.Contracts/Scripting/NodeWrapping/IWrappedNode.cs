using Laminar.Contracts.Primitives;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IWrappedNode : INotificationClient<LaminarExecutionContext>
{
    Identifier<IWrappedNode> Id { get; }

    INodeRow NameRow { get; }

    IReadOnlyObservableCollection<INodeRow> Rows { get; }

    ObservableValue<Point> Location { get; }

    void Update(LaminarExecutionContext context);

    IWrappedNode Clone(INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient);
}
