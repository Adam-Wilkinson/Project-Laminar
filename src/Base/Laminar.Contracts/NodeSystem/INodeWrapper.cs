using System.Collections.ObjectModel;
using Laminar.Contracts.Primitives;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.NodeSystem;

public interface INodeWrapper : INotificationClient<LaminarExecutionContext>
{
    Identifier<INodeWrapper> Id { get; }

    INodeRowWrapper NameRow { get; }

    ObservableCollection<INodeRowWrapper> Fields { get; }

    ObservableValue<Point> Location { get; }

    void Update(LaminarExecutionContext context);

    INodeWrapper Clone(INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient);
}
