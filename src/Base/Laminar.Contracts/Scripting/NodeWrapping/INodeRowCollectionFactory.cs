using System.Collections.ObjectModel;
using Laminar.Contracts.Primitives;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeRowCollectionFactory
{
    public IReadOnlyObservableCollection<IWrappedNodeRow> CreateNodeRowsForObject(object toWrap, INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient);

    public void CopyNodeRowValues<T>(T from, T to) where T : IWrappedNode;
}
