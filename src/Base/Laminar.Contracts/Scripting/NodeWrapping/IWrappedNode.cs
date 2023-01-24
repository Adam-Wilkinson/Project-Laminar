using Laminar.Contracts.Primitives;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IWrappedNode : INotificationClient<LaminarExecutionContext>
{
    Identifier<IWrappedNode> Id { get; }

    IWrappedNodeRow NameRow { get; }

    IReadOnlyObservableCollection<IWrappedNodeRow> Rows { get; }

    ObservableValue<Point> Location { get; }

    void Update(LaminarExecutionContext context);

    IWrappedNode Clone(INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient);
}
