using System.Collections.ObjectModel;
using System.Reflection;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Core.ScriptEditor.Nodes;

internal class NodeRowCollectionFactory : INodeRowCollectionFactory
{
    readonly INodeRowWrapperFactory _nodeRowWrapperFactory;

    public NodeRowCollectionFactory(INodeRowWrapperFactory factory)
    {
        _nodeRowWrapperFactory = factory;
    }

    public void CopyNodeRowValues<T>(T from, T to) where T : INodeWrapper
    {
        for (int i = 0; i < from.Fields.Count; i++)
        {
            from.Fields[i].CloneTo(to.Fields[i]);
        }
    }

    public ObservableCollection<INodeRowWrapper> CreateNodeRowsForObject(object toWrap, INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient)
    {
        var output = new ObservableCollection<INodeRowWrapper>();

        foreach (var field in toWrap.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            if (_nodeRowWrapperFactory.TryCreateNodeRowFromMember(field, toWrap, out INodeRowWrapper nodeRowWrapper, userChangedValueNotificationClient))
            {
                output.Add(nodeRowWrapper);
            }
        }

        return output;
    }
}
