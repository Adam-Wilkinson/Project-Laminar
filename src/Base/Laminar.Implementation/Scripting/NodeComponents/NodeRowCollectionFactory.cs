using System;
using System.Collections.Generic;
using System.Reflection;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;

namespace Laminar.Implementation.Scripting.NodeComponents;

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
            if (field.MemberType == MemberTypes.Field && field.GetCustomAttribute<ShowInNodeAttribute>() is not null)
            {
                object fieldValue = ((FieldInfo)field).GetValue(toWrap);
                if (fieldValue is INodeRow nodeRow)
                {
                    output.Add(nodeRow);
                }
                else if (fieldValue is INodeComponent nodeComponent)
                {
                    output.Add(nodeComponent);
                }
            }
        }

        return new FlattenedObservableTree<INodeRow>(output);
    }
}
