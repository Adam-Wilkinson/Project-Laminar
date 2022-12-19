using System.Collections.ObjectModel;
using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.NodeSystem;

public interface INodeRowCollectionFactory
{
    public ObservableCollection<INodeRowWrapper> CreateNodeRowsForObject(object toWrap, INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient);

    public void CopyNodeRowValues<T>(T from, T to) where T : INodeWrapper;
}
