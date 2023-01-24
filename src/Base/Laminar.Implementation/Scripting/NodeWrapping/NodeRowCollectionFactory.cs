using System.Collections.Generic;
using System.Reflection;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Nodes;

internal class NodeRowCollectionFactory : INodeRowCollectionFactory
{
    readonly INodeRowWrapperFactory _nodeRowWrapperFactory;

    public NodeRowCollectionFactory(INodeRowWrapperFactory factory)
    {
        _nodeRowWrapperFactory = factory;
    }

    public void CopyNodeRowValues<T>(T from, T to) where T : IWrappedNode
    {
        for (int i = 0; i < from.Rows.Count; i++)
        {
            from.Rows[i].CloneTo(to.Rows[i]);
        }
    }

    public IReadOnlyObservableCollection<IWrappedNodeRow> CreateNodeRowsForObject(object toWrap, INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient)
    {
        var output = new List<IWrappedNodeRow>();

        foreach (var field in toWrap.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            if (_nodeRowWrapperFactory.TryCreateNodeRowFromMember(field, toWrap, out IWrappedNodeRow nodeRowWrapper, userChangedValueNotificationClient))
            {
                output.Add(nodeRowWrapper);
            }
        }

        return new FlattenedObservableTree<IWrappedNodeRow>(output);
    }
}
