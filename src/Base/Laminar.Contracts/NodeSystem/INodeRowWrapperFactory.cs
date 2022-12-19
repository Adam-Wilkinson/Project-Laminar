using System.Reflection;
using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.NodeSystem;

public interface INodeRowWrapperFactory
{
    public INodeRowWrapper CreateNodeRowWrapper(NodeRow row, INotificationClient<LaminarExecutionContext>? userChangedValueNotifiee);

    public bool TryCreateNodeRowFromMember(MemberInfo fieldInfo, object containingObject, out INodeRowWrapper nodeRowWrapper, INotificationClient<LaminarExecutionContext>? userChangedValueNotifiee);
}
