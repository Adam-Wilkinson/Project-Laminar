using System;
using System.Collections.Generic;
using System.Reflection;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Implementation.Scripting.Nodes;

internal class NodeRowCollectionFactory : INodeRowCollectionFactory
{
    public void CopyNodeRowValues<T>(T from, T to) where T : IWrappedNode
    {
        for (int i = 0; i < from.Rows.Count; i++)
        {
            from.Rows[i].CopyValueTo(to.Rows[i]);
        }
    }

    public IReadOnlyObservableCollection<INodeRow> CreateNodeRowsForObject(object toWrap, INotificationClient<LaminarExecutionContext> userChangedValueNotificationClient)
    {
        List<object> output = new();

        foreach (MemberInfo field in toWrap.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.MemberType == MemberTypes.Field)
            {
                object fieldValue = (((FieldInfo)field).GetValue(toWrap));
                if (fieldValue is NodeComponent nodeComponent)
                {
                    output.Add(nodeComponent.Component);
                }
                else if (fieldValue is IConvertsToNodeComponent converts)
                {
                    output.Add(converts.GetComponent().Component);
                }
            }
        }

        return new FlattenedObservableTree<INodeRow>(output);
    }
}
