using System.Collections.ObjectModel;
using System.Reflection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Primitives;
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
        for (int i = 0; i < from.Fields.Count; i++)
        {
            from.Fields[i].CloneTo(to.Fields[i]);
        }
    }

    public ObservableCollection<IWrappedNodeRow> CreateNodeRowsForObject(object toWrap, INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient)
    {
        var output = new ObservableCollection<IWrappedNodeRow>();

        foreach (var field in toWrap.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            if (_nodeRowWrapperFactory.TryCreateNodeRowFromMember(field, toWrap, out IWrappedNodeRow nodeRowWrapper, userChangedValueNotificationClient))
            {
                output.Add(nodeRowWrapper);
            }
        }

        return output;
    }
}
