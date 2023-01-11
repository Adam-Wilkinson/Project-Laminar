using System.Reflection;
using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeRowWrapperFactory
{
    public IWrappedNodeRow CreateNodeRowWrapper(NodeRow row, INotificationClient<LaminarExecutionContext>? userChangedValueNotifiee);

    public bool TryCreateNodeRowFromMember(MemberInfo fieldInfo, object containingObject, out IWrappedNodeRow nodeRowWrapper, INotificationClient<LaminarExecutionContext>? userChangedValueNotifiee);
}
